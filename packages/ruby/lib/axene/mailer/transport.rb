# frozen_string_literal: true

require "net/http"
require "json"
require "securerandom"
require "uri"

require_relative "error"

module Axene
  module Mailer
    # HTTP transport: the single place that talks to the network. Owns bearer
    # authentication, JSON encode/decode, timeouts, retries with backoff, and
    # turning non-2xx responses into {Axene::Mailer::Error}. Resources depend on
    # this, not on Net::HTTP directly.
    class Transport
      DEFAULT_BASE_URL = "https://mail.axene.io"
      USER_AGENT = "axene-mailer-ruby/#{VERSION}"

      # @param api_key [String] required; starts with "axm_k_"
      # @param base_url [String] default "https://mail.axene.io"
      # @param max_retries [Integer] retry attempts for 429/5xx, default 3
      # @param timeout [Numeric] per-request timeout in seconds, default 30
      def initialize(api_key:, base_url: DEFAULT_BASE_URL, max_retries: 3, timeout: 30)
        raise ArgumentError, "Axene::Mailer: `api_key` is required." if api_key.nil? || api_key.empty?

        @api_key = api_key
        @base_url = base_url.sub(%r{/+\z}, "")
        @max_retries = max_retries
        @timeout = timeout
      end

      # Perform a JSON request and return the parsed body (symbolized keys).
      #
      # Retries 429 and 5xx with exponential backoff, honoring Retry-After when
      # present. Raises {Axene::Mailer::Error} on a final non-2xx or a transport
      # failure that survives every attempt.
      #
      # @param method [Symbol] :get, :post, :patch, :put, :delete
      # @param path [String] path beginning with "/"
      # @param body [Object, nil] request body, JSON-encoded when present
      # @param query [Hash, nil] query parameters (nil values dropped)
      # @return [Hash, Array, nil]
      def request(method, path, body: nil, query: nil)
        uri = build_uri(path, query)
        last_error = nil

        (1..@max_retries).each do |attempt|
          req = build_request(method, uri)
          unless body.nil?
            req["Content-Type"] = "application/json"
            req.body = JSON.generate(body)
          end

          begin
            res = http(uri).request(req)
          rescue StandardError => e
            last_error = e
            sleep(backoff_seconds(nil, attempt)) if attempt < @max_retries
            next
          end

          status = res.code.to_i
          if retryable?(status) && attempt < @max_retries
            sleep(backoff_seconds(res, attempt))
            next
          end

          payload = parse_body(res)
          raise to_error(status, payload) unless status.between?(200, 299)

          return payload
        end

        raise Error.new(0, "Axene::Mailer request failed: #{last_error}")
      end

      # Upload a single file as multipart/form-data under the field name "file".
      # Used by the CSV/suppression import endpoints. Not retried (uploads are
      # not idempotent).
      #
      # @param path [String]
      # @param file_bytes [String] raw file contents
      # @param filename [String]
      # @return [Hash, Array, nil]
      def upload(path, file_bytes, filename)
        uri = build_uri(path, nil)
        boundary = "AxeneBoundary#{SecureRandom.hex(16)}"
        req = build_request(:post, uri)
        req["Content-Type"] = "multipart/form-data; boundary=#{boundary}"
        req.body = multipart_body(boundary, file_bytes, filename)

        res = http(uri).request(req)
        status = res.code.to_i
        payload = parse_body(res)
        raise to_error(status, payload) unless status.between?(200, 299)

        payload
      end

      private

      def build_uri(path, query)
        uri = URI.parse("#{@base_url}#{path}")
        if query && !query.empty?
          pairs = query.reject { |_, v| v.nil? }.map { |k, v| [k.to_s, v.to_s] }
          uri.query = URI.encode_www_form(pairs) unless pairs.empty?
        end
        uri
      end

      def build_request(method, uri)
        klass = {
          get: Net::HTTP::Get,
          post: Net::HTTP::Post,
          patch: Net::HTTP::Patch,
          put: Net::HTTP::Put,
          delete: Net::HTTP::Delete
        }.fetch(method)
        req = klass.new(uri)
        req["Authorization"] = "Bearer #{@api_key}"
        req["Accept"] = "application/json"
        req["User-Agent"] = USER_AGENT
        req
      end

      def http(uri)
        h = Net::HTTP.new(uri.host, uri.port)
        h.use_ssl = uri.scheme == "https"
        h.open_timeout = @timeout
        h.read_timeout = @timeout
        h
      end

      def multipart_body(boundary, file_bytes, filename)
        safe_name = filename.to_s.gsub('"', "")
        [
          "--#{boundary}\r\n",
          %(Content-Disposition: form-data; name="file"; filename="#{safe_name}"\r\n),
          "Content-Type: application/octet-stream\r\n\r\n",
          file_bytes.dup.force_encoding(Encoding::ASCII_8BIT),
          "\r\n--#{boundary}--\r\n"
        ].join
      end

      def retryable?(status)
        status == 429 || status >= 500
      end

      def backoff_seconds(res, attempt)
        if res
          retry_after = res["retry-after"].to_f
          return retry_after if retry_after.positive?
        end
        0.25 * (2**(attempt - 1))
      end

      def parse_body(res)
        ctype = res["content-type"].to_s
        return nil unless ctype.include?("application/json")

        raw = res.body
        return nil if raw.nil? || raw.empty?

        JSON.parse(raw, symbolize_names: true)
      rescue JSON::ParserError
        nil
      end

      # Map the API's { detail: { code, message } } (or a string detail) into an
      # {Axene::Mailer::Error}.
      def to_error(status, payload)
        detail = payload.is_a?(Hash) ? payload[:detail] : nil
        code = detail.is_a?(Hash) ? detail[:code] : nil
        message =
          if detail.is_a?(Hash)
            detail[:message]
          elsif detail.is_a?(String)
            detail
          end
        message ||= "Axene::Mailer request failed (#{status})"
        Error.new(status, message, code, payload)
      end
    end
  end
end
