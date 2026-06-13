# frozen_string_literal: true

require "minitest/autorun"
require "socket"
require "json"
require "axene/mailer"

# A tiny in-process HTTP server backed by a raw TCPServer so the tests have zero
# external gem dependencies (no WebMock). Each instance records the requests it
# receives and serves responses from a queue. It speaks just enough HTTP/1.1 to
# satisfy Net::HTTP.
class MockServer
  Request = Struct.new(:method, :path, :query, :headers, :body)

  attr_reader :requests

  def initialize
    @server = TCPServer.new("127.0.0.1", 0)
    @responses = []
    @requests = []
    @thread = Thread.new { serve_loop }
  end

  def port
    @server.addr[1]
  end

  def base_url
    "http://127.0.0.1:#{port}"
  end

  # Queue a response. Bodies that are not Strings are JSON-encoded.
  def enqueue(status: 200, body: nil, headers: {})
    payload =
      if body.nil?
        ""
      elsif body.is_a?(String)
        body
      else
        JSON.generate(body)
      end
    hdrs = { "Content-Type" => "application/json" }.merge(headers)
    @responses << [status, hdrs, payload]
  end

  def shutdown
    @server.close
    @thread.kill
  rescue StandardError
    nil
  end

  private

  def serve_loop
    loop do
      client = @server.accept
      handle(client)
      client.close
    rescue IOError, Errno::EBADF
      break
    rescue StandardError
      next
    end
  end

  def handle(client)
    request_line = client.gets
    return if request_line.nil?

    method, target, = request_line.split(" ")
    path, query = target.split("?", 2)

    headers = {}
    while (line = client.gets) && line != "\r\n"
      key, value = line.chomp.split(": ", 2)
      headers[key.downcase] = value if key
    end

    body = nil
    if (len = headers["content-length"])
      body = client.read(len.to_i)
    end

    @requests << Request.new(method, path, query, headers, body)

    status, resp_headers, payload = @responses.shift || [200, { "Content-Type" => "application/json" }, "{}"]
    write_response(client, status, resp_headers, payload)
  end

  STATUS_TEXT = {
    200 => "OK", 201 => "Created", 202 => "Accepted", 204 => "No Content",
    400 => "Bad Request", 429 => "Too Many Requests", 500 => "Internal Server Error"
  }.freeze

  def write_response(client, status, headers, payload)
    reason = STATUS_TEXT.fetch(status, "OK")
    client.write("HTTP/1.1 #{status} #{reason}\r\n")
    client.write("Content-Length: #{payload.bytesize}\r\n")
    headers.each { |k, v| client.write("#{k}: #{v}\r\n") }
    client.write("Connection: close\r\n\r\n")
    client.write(payload)
  end
end
