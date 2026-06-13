<?php

declare(strict_types=1);

namespace Axene\Mailer\Tests;

use Axene\Mailer\AxeneException;
use Axene\Mailer\Client;
use GuzzleHttp\Client as GuzzleClient;
use GuzzleHttp\Handler\MockHandler;
use GuzzleHttp\HandlerStack;
use GuzzleHttp\Middleware;
use GuzzleHttp\Psr7\Request;
use GuzzleHttp\Psr7\Response;
use PHPUnit\Framework\TestCase;

/**
 * Mock-transport tests. No network: a Guzzle MockHandler returns canned
 * responses and a history middleware captures the outgoing requests so we can
 * assert on the wire shape.
 */
final class ClientTest extends TestCase
{
    /** @var list<array{request: Request}> */
    private array $history = [];

    /**
     * Build a client whose Guzzle transport replays the given queued responses
     * and records every outgoing request into {@see self::$history}.
     *
     * @param list<Response> $responses
     */
    private function clientWith(array $responses): Client
    {
        $this->history = [];
        $mock = new MockHandler($responses);
        $stack = HandlerStack::create($mock);
        $stack->push(Middleware::history($this->history));
        $guzzle = new GuzzleClient(['handler' => $stack]);

        return new Client('axm_k_test123', ['http' => $guzzle]);
    }

    private function lastRequest(): Request
    {
        /** @var Request $request */
        $request = $this->history[array_key_last($this->history)]['request'];

        return $request;
    }

    /**
     * @param array<string, mixed> $payload
     */
    private function jsonResponse(int $status, array|string $payload): Response
    {
        return new Response($status, ['Content-Type' => 'application/json'], json_encode($payload));
    }

    public function testSendsBearerAuthorizationHeader(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(202, ['id' => 'em_1', 'status' => 'queued']),
        ]);

        $client->emails->send([
            'from' => 'hello@yourdomain.com',
            'to' => 'customer@example.com',
            'subject' => 'Hi',
            'html' => '<p>hi</p>',
        ]);

        $this->assertSame('Bearer axm_k_test123', $this->lastRequest()->getHeaderLine('Authorization'));
    }

    public function testMapsFromToFromUnderscoreAndAddressSugar(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(202, ['id' => 'em_1', 'status' => 'queued']),
        ]);

        $client->emails->send([
            'from' => 'hello@yourdomain.com',
            'to' => 'customer@example.com',
            'subject' => 'Hi',
        ]);

        $body = json_decode((string) $this->lastRequest()->getBody(), true);

        $this->assertArrayHasKey('from_', $body);
        $this->assertArrayNotHasKey('from', $body);
        $this->assertSame(['email' => 'hello@yourdomain.com'], $body['from_']);
        // Bare string `to` becomes a list of address objects.
        $this->assertSame([['email' => 'customer@example.com']], $body['to']);
        // Null fields are dropped.
        $this->assertArrayNotHasKey('html', $body);
        $this->assertArrayNotHasKey('cc', $body);
    }

    public function testSendBatchPostsBareArray(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(202, ['total' => 2, 'sent' => 2, 'failed' => 0, 'results' => []]),
        ]);

        $client->emails->sendBatch([
            ['from' => 'a@x.io', 'to' => 'b@x.io', 'subject' => 'One'],
            ['from' => 'a@x.io', 'to' => 'c@x.io', 'subject' => 'Two'],
        ]);

        $request = $this->lastRequest();
        $this->assertSame('/v1/emails/batch', $request->getUri()->getPath());
        $body = json_decode((string) $request->getBody(), true);
        $this->assertTrue(array_is_list($body), 'batch body must be a bare array');
        $this->assertCount(2, $body);
        $this->assertSame(['email' => 'a@x.io'], $body[0]['from_']);
        $this->assertSame('Two', $body[1]['subject']);
    }

    public function testValidateSendsFullBody(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(200, ['valid' => true, 'can_send' => true, 'issues' => []]),
        ]);

        $client->emails->validate([
            'from' => ['email' => 'a@x.io', 'name' => 'A'],
            'to' => [['email' => 'b@x.io'], 'c@x.io'],
            'cc' => 'd@x.io',
            'subject' => 'Full',
            'html' => '<p>h</p>',
            'text' => 't',
            'replyTo' => 'reply@x.io',
            'tags' => ['welcome'],
        ]);

        $request = $this->lastRequest();
        $this->assertSame('/v1/emails/validate', $request->getUri()->getPath());
        $body = json_decode((string) $request->getBody(), true);

        $this->assertSame(['email' => 'a@x.io', 'name' => 'A'], $body['from_']);
        $this->assertSame([['email' => 'b@x.io'], ['email' => 'c@x.io']], $body['to']);
        $this->assertSame([['email' => 'd@x.io']], $body['cc']);
        $this->assertSame(['email' => 'reply@x.io'], $body['reply_to']);
        $this->assertSame('<p>h</p>', $body['html']);
        $this->assertSame('t', $body['text']);
        $this->assertSame(['welcome'], $body['tags']);
    }

    public function testUploadCsvSendsMultipartFileField(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(200, ['imported' => 3, 'skipped' => 0, 'errors' => []]),
        ]);

        $result = $client->contacts->uploadCsv('list_1', "email,name\na@x.io,A\n", 'people.csv');

        $this->assertSame(3, $result['imported']);
        $request = $this->lastRequest();
        $this->assertSame('/v1/contacts/list_1/upload', $request->getUri()->getPath());

        $contentType = $request->getHeaderLine('Content-Type');
        $this->assertStringContainsString('multipart/form-data', $contentType);

        $rawBody = (string) $request->getBody();
        $this->assertStringContainsString('name="file"', $rawBody);
        $this->assertStringContainsString('filename="people.csv"', $rawBody);
        $this->assertStringContainsString('a@x.io,A', $rawBody);
    }

    public function testSuppressionsListParsesEnvelopeAndAddMapsEmailAddress(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(200, [
                'items' => [['id' => 's_1', 'email_address' => 'spam@x.io', 'reason' => 'manual']],
                'total' => 1,
                'page' => 0,
                'limit' => 50,
            ]),
            $this->jsonResponse(201, ['id' => 's_2', 'email_address' => 'bad@x.io', 'reason' => 'manual']),
        ]);

        $page = $client->suppressions->list(['page' => 0, 'limit' => 50]);
        $this->assertSame(1, $page['total']);
        $this->assertSame('spam@x.io', $page['items'][0]['email_address']);

        $client->suppressions->add(['email' => 'bad@x.io']);
        $body = json_decode((string) $this->lastRequest()->getBody(), true);
        $this->assertSame('bad@x.io', $body['email_address']);
        $this->assertArrayNotHasKey('email', $body);
        $this->assertSame('manual', $body['reason']);
    }

    public function testWebhookUpdateMapsIsActive(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(200, ['id' => 'wh_1', 'url' => 'https://x.io/hook', 'is_active' => false]),
        ]);

        $client->webhooks->update('wh_1', ['isActive' => false]);

        $request = $this->lastRequest();
        $this->assertSame('PATCH', $request->getMethod());
        $body = json_decode((string) $request->getBody(), true);
        $this->assertArrayHasKey('is_active', $body);
        $this->assertFalse($body['is_active']);
        $this->assertArrayNotHasKey('isActive', $body);
    }

    public function testRetriesOn429ThenSucceeds(): void
    {
        $client = $this->clientWith([
            new Response(429, ['Retry-After' => '0', 'Content-Type' => 'application/json'], '{"detail":"slow down"}'),
            $this->jsonResponse(200, [['id' => 'em_1', 'status' => 'sent']]),
        ]);

        $emails = $client->emails->list();

        $this->assertCount(1, $emails);
        $this->assertSame('em_1', $emails[0]['id']);
        $this->assertCount(2, $this->history, 'should have retried once');
    }

    public function testMapsErrorEnvelopeToAxeneException(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(422, ['detail' => ['code' => 'unverified_sender', 'message' => 'Sender not verified']]),
        ]);

        try {
            $client->emails->send(['from' => 'a@x.io', 'to' => 'b@x.io', 'subject' => 'Hi']);
            $this->fail('expected AxeneException');
        } catch (AxeneException $e) {
            $this->assertSame(422, $e->getStatus());
            $this->assertSame('unverified_sender', $e->getCode());
            $this->assertSame('Sender not verified', $e->getMessage());
        }
    }

    public function testZeroBasedPageInQueryString(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(200, []),
        ]);

        $client->emails->list(['page' => 0, 'limit' => 20]);

        $query = $this->lastRequest()->getUri()->getQuery();
        $this->assertStringContainsString('page=0', $query);
        $this->assertStringContainsString('limit=20', $query);
    }

    public function testBulkSendInjectsContactListId(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(200, ['queued' => 5, 'skipped' => 0, 'errors' => []]),
        ]);

        $client->contacts->bulkSend('list_42', [
            'senderAddressId' => 'sa_1',
            'subject' => 'Hello {{name}}',
            'html' => '<p>hi</p>',
        ]);

        $body = json_decode((string) $this->lastRequest()->getBody(), true);
        $this->assertSame('list_42', $body['contact_list_id']);
        $this->assertSame('sa_1', $body['sender_address_id']);
        $this->assertArrayNotHasKey('text', $body, 'null text dropped');
    }

    public function testTemplateCreateMapsHtmlAndTextToBodies(): void
    {
        $client = $this->clientWith([
            $this->jsonResponse(201, ['id' => 'tpl_1', 'name' => 'Welcome']),
        ]);

        $client->templates->create([
            'name' => 'Welcome',
            'html' => '<p>{{name}}</p>',
            'text' => 'hi {{name}}',
        ]);

        $body = json_decode((string) $this->lastRequest()->getBody(), true);
        $this->assertSame('<p>{{name}}</p>', $body['html_body']);
        $this->assertSame('hi {{name}}', $body['text_body']);
        $this->assertArrayNotHasKey('html', $body);
        $this->assertArrayNotHasKey('text', $body);
    }
}
