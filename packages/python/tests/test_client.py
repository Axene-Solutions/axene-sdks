"""Integration-style tests against a local HTTP server (stdlib only)."""

import json
import threading
import unittest
from http.server import BaseHTTPRequestHandler, HTTPServer

from axene_mailer import Axene, AxeneError


class _Handler(BaseHTTPRequestHandler):
    def _handle(self):
        srv = self.server
        length = int(self.headers.get("Content-Length", 0))
        body = self.rfile.read(length).decode() if length else ""
        srv.requests.append({
            "method": self.command,
            "path": self.path,
            "auth": self.headers.get("Authorization"),
            "body": body,
        })
        i = min(srv.call, len(srv.statuses) - 1)
        srv.call += 1
        status = srv.statuses[i]
        payload = (srv.response if 200 <= status < 300
                   else '{"detail":{"code":"invalid","message":"bad from"}}').encode()
        self.send_response(status)
        self.send_header("Content-Type", "application/json")
        self.send_header("Content-Length", str(len(payload)))
        self.end_headers()
        self.wfile.write(payload)

    do_GET = _handle
    do_POST = _handle

    def log_message(self, *args):  # silence the test server
        pass


class ClientTest(unittest.TestCase):
    def setUp(self):
        self.server = HTTPServer(("127.0.0.1", 0), _Handler)
        self.server.requests = []
        self.server.statuses = [202]
        self.server.response = '{"id":"em_1","status":"queued"}'
        self.server.call = 0
        threading.Thread(target=self.server.serve_forever, daemon=True).start()
        self.base = f"http://127.0.0.1:{self.server.server_address[1]}"

    def tearDown(self):
        self.server.shutdown()
        self.server.server_close()

    def client(self):
        return Axene(api_key="axm_k_test", base_url=self.base, max_retries=3)

    def test_send_maps_from_and_sets_bearer(self):
        res = self.client().emails.send({
            "from": {"email": "hello@shop.co", "name": "Shop"},
            "to": "a@example.com",
            "subject": "Hi",
            "html": "<p>x</p>",
        })
        self.assertEqual(res["id"], "em_1")
        req = self.server.requests[0]
        self.assertEqual(req["path"], "/v1/emails/")
        self.assertEqual(req["auth"], "Bearer axm_k_test")
        body = json.loads(req["body"])
        self.assertEqual(body["from_"], {"email": "hello@shop.co", "name": "Shop"})
        self.assertNotIn("from", body)
        self.assertEqual(body["to"], [{"email": "a@example.com"}])
        self.assertNotIn("text", body)  # nulls pruned

    def test_non_2xx_raises(self):
        self.server.statuses = [422]
        with self.assertRaises(AxeneError) as cm:
            self.client().emails.send({"from": "f@x.co", "to": "a@x.co", "subject": "s"})
        self.assertEqual(cm.exception.status, 422)
        self.assertEqual(cm.exception.code, "invalid")

    def test_retries_5xx_then_succeeds(self):
        self.server.statuses = [503, 503, 202]
        res = self.client().emails.send({"from": "f@x.co", "to": "a@x.co", "subject": "s"})
        self.assertEqual(res["id"], "em_1")
        self.assertEqual(len(self.server.requests), 3)

    def test_send_batch_posts_bare_array(self):
        self.server.response = '{"total":1,"sent":1,"failed":0,"results":[{"id":"a","status":"queued"}]}'
        res = self.client().emails.send_batch([{"from": "f@x.co", "to": "a@x.co", "subject": "s"}])
        self.assertEqual(res["total"], 1)
        body = json.loads(self.server.requests[0]["body"])
        self.assertIsInstance(body, list)  # bare array, not {"emails": [...]}
        self.assertEqual(body[0]["from_"], {"email": "f@x.co"})

    def test_validate_posts_full_message(self):
        self.server.response = '{"valid":true,"can_send":true,"issues":[],"plan":"free","usage":{}}'
        res = self.client().emails.validate({"from": "f@x.co", "to": "a@x.co", "subject": "s"})
        self.assertTrue(res["can_send"])
        self.assertEqual(self.server.requests[0]["path"], "/v1/emails/validate")
        body = json.loads(self.server.requests[0]["body"])
        self.assertEqual(body["from_"], {"email": "f@x.co"})  # full send body

    def test_list_domains(self):
        self.server.response = '[{"id":"d1","name":"shop.co","status":"verified"}]'
        self.server.statuses = [200]
        domains = self.client().domains.list()
        self.assertEqual(domains[0]["name"], "shop.co")


if __name__ == "__main__":
    unittest.main()
