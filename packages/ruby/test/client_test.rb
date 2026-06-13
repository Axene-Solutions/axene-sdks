# frozen_string_literal: true

require_relative "test_helper"

class ClientTest < Minitest::Test
  def setup
    @server = MockServer.new
    @client = Axene::Mailer::Client.new(api_key: "axm_k_test", base_url: @server.base_url, max_retries: 3)
  end

  def teardown
    @server.shutdown
  end

  def last_request
    @server.requests.last
  end

  def last_body
    JSON.parse(last_request.body, symbolize_names: true)
  end

  def test_bearer_header_is_sent
    @server.enqueue(status: 200, body: [])
    @client.emails.list
    assert_equal "Bearer axm_k_test", last_request.headers["authorization"]
  end

  def test_send_maps_from_to_from_underscore_and_address_sugar
    @server.enqueue(status: 202, body: { id: "em_1", status: "queued" })
    result = @client.emails.send(
      from: "hi@you.io",
      to: "person@example.com",
      subject: "Hello",
      html: "<p>hi</p>"
    )
    body = last_body
    assert_equal({ email: "hi@you.io" }, body[:from_])
    refute body.key?(:from)
    assert_equal [{ email: "person@example.com" }], body[:to]
    assert_equal "em_1", result[:id]
  end

  def test_send_accepts_named_addresses_and_drops_nil_keys
    @server.enqueue(status: 202, body: { id: "em_2", status: "queued" })
    @client.emails.send(
      from: { email: "hi@you.io", name: "You" },
      to: [{ email: "a@example.com", name: "A" }],
      subject: "Hi"
    )
    body = last_body
    assert_equal({ email: "hi@you.io", name: "You" }, body[:from_])
    refute body.key?(:html)
    refute body.key?(:text)
    refute body.key?(:cc)
  end

  def test_send_batch_posts_a_bare_array
    @server.enqueue(status: 202, body: { total: 2, sent: 2, failed: 0, results: [] })
    @client.emails.send_batch([
                                { from: "a@you.io", to: "x@example.com", subject: "1" },
                                { from: "a@you.io", to: "y@example.com", subject: "2" }
                              ])
    body = JSON.parse(last_request.body, symbolize_names: true)
    assert_kind_of Array, body
    assert_equal 2, body.length
    assert_equal({ email: "a@you.io" }, body[0][:from_])
    assert_equal "/v1/emails/batch", last_request.path
  end

  def test_validate_sends_full_send_body
    @server.enqueue(status: 200, body: { valid: true, can_send: true, issues: [] })
    @client.emails.validate(from: "a@you.io", to: "x@example.com", subject: "Check", text: "body")
    body = last_body
    assert_equal "/v1/emails/validate", last_request.path
    assert_equal({ email: "a@you.io" }, body[:from_])
    assert_equal [{ email: "x@example.com" }], body[:to]
    assert_equal "Check", body[:subject]
    assert_equal "body", body[:text]
  end

  def test_list_uses_zero_based_page_query
    @server.enqueue(status: 200, body: [])
    @client.emails.list(status: "delivered", page: 0, limit: 20)
    q = last_request.query
    assert_includes q, "page=0"
    assert_includes q, "status=delivered"
  end

  def test_multipart_csv_upload_uses_field_file
    @server.enqueue(status: 200, body: { imported: 3, skipped: 0, errors: [] })
    result = @client.contacts.upload_csv("list_1", "email\na@example.com\n", filename: "people.csv")
    req = last_request
    assert_equal "/v1/contacts/list_1/upload", req.path
    assert_includes req.headers["content-type"], "multipart/form-data"
    assert_includes req.headers["content-type"], "boundary="
    assert_includes req.body, %(name="file")
    assert_includes req.body, %(filename="people.csv")
    assert_includes req.body, "a@example.com"
    assert_equal 3, result[:imported]
  end

  def test_suppressions_envelope_and_email_address_mapping
    @server.enqueue(status: 200, body: { items: [{ id: "s1", email_address: "x@example.com", reason: "manual" }],
                                         total: 1, page: 0, limit: 50 })
    page = @client.suppressions.list(page: 0, limit: 50)
    assert_equal 1, page[:total]
    assert_equal "x@example.com", page[:items][0][:email_address]

    @server.enqueue(status: 201, body: { id: "s2", email_address: "y@example.com", reason: "manual" })
    @client.suppressions.add(email: "y@example.com")
    body = last_body
    assert_equal "y@example.com", body[:email_address]
    refute body.key?(:email)
    assert_equal "manual", body[:reason]
  end

  def test_webhooks_is_active_mapping
    @server.enqueue(status: 200, body: { id: "wh_1", url: "https://x.io", events: [], is_active: false })
    @client.webhooks.update("wh_1", is_active: false)
    body = last_body
    assert_equal false, body[:is_active]
    refute body.key?(:isActive)
  end

  def test_templates_html_text_map_to_body_fields
    @server.enqueue(status: 201, body: { id: "tpl_1", name: "Welcome" })
    @client.templates.create(name: "Welcome", html: "<p>hi</p>", text: "hi")
    body = last_body
    assert_equal "<p>hi</p>", body[:html_body]
    assert_equal "hi", body[:text_body]
    refute body.key?(:html)
    refute body.key?(:text)
  end

  def test_contacts_bulk_send_injects_contact_list_id
    @server.enqueue(status: 200, body: { queued: 5, skipped: 0, errors: [] })
    @client.contacts.bulk_send("list_9", sender_address_id: "sa_1", subject: "Hi")
    body = last_body
    assert_equal "list_9", body[:contact_list_id]
    assert_equal "sa_1", body[:sender_address_id]
  end

  def test_retry_then_success_on_429
    @server.enqueue(status: 429, body: { detail: "slow down" }, headers: { "Retry-After" => "0" })
    @server.enqueue(status: 200, body: [])
    result = @client.emails.list
    assert_equal [], result
    assert_equal 2, @server.requests.length
  end

  def test_error_mapping_from_detail_object
    @server.enqueue(status: 400, body: { detail: { code: "invalid", message: "bad input" } })
    err = assert_raises(Axene::Mailer::Error) { @client.emails.list }
    assert_equal 400, err.status
    assert_equal "invalid", err.code
    assert_equal "bad input", err.message
  end
end
