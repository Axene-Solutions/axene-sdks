# frozen_string_literal: true

module Axene
  module Mailer
    module Resources
      # The +emails+ resource: send, look up, search, schedule, and inspect
      # messages. Accessed as +client.emails+.
      class Emails
        # @param transport [Axene::Mailer::Transport]
        def initialize(transport)
          @transport = transport
        end

        # Send a single email.
        #
        # Accepts keyword arguments or a Hash. The +from+ field is exposed
        # cleanly and mapped to the wire name +from_+. A bare String is accepted
        # anywhere an address is expected and becomes +{ email: ... }+.
        #
        # @param message [Hash] send parameters (:from, :to, :subject, :html, ...)
        # @return [Hash] { id:, status:, message_id:, rejection_reason: }
        def send(message = {}, **kwargs)
          body = serialize_send(message.empty? ? kwargs : message)
          @transport.request(:post, "/v1/emails/", body: body)
        end

        # Send up to your plan's batch limit in one call. The API accepts a bare
        # array of messages and returns a per-message result set.
        #
        # @param messages [Array<Hash>]
        # @return [Hash] { total:, sent:, failed:, results: [...] }
        def send_batch(messages)
          @transport.request(:post, "/v1/emails/batch", body: messages.map { |m| serialize_send(m) })
        end

        # Dry-run a send: check whether +message+ would be accepted without
        # actually sending it. Uses the full send body.
        #
        # @param message [Hash]
        # @return [Hash] { valid:, can_send:, issues:, plan:, usage: }
        def validate(message = {}, **kwargs)
          body = serialize_send(message.empty? ? kwargs : message)
          @transport.request(:post, "/v1/emails/validate", body: body)
        end

        # List recent emails, newest first.
        #
        # @param status [String, nil]
        # @param page [Integer] zero-based, default 0
        # @param limit [Integer] default 20
        # @return [Array<Hash>]
        def list(status: nil, page: 0, limit: 20)
          @transport.request(:get, "/v1/emails/", query: { status: status, page: page, limit: limit })
        end

        # Fetch a single email with its bodies and events.
        #
        # @param id [String]
        # @return [Hash]
        def get(id)
          @transport.request(:get, "/v1/emails/#{escape(id)}")
        end

        # List delivery / open / click / bounce events for an email.
        #
        # @param id [String]
        # @return [Array<Hash>]
        def events(id)
          @transport.request(:get, "/v1/emails/#{escape(id)}/events")
        end

        # Re-send a bounced, rejected, or failed email as a new message.
        #
        # @param id [String]
        # @return [Hash]
        def retry(id)
          @transport.request(:post, "/v1/emails/#{escape(id)}/retry")
        end

        # Search emails. +q+ supports inline tokens (to:, from:, status:,
        # domain:, tag:); leftover words are matched as free text.
        #
        # @param q [String, nil]
        # @param status [String, nil]
        # @param tag [String, nil]
        # @param page [Integer] zero-based, default 0
        # @param limit [Integer] default 20
        # @return [Array<Hash>]
        def search(q: nil, status: nil, tag: nil, page: 0, limit: 20)
          @transport.request(:get, "/v1/emails/search",
                             query: { q: q, status: status, tag: tag, page: page, limit: limit })
        end

        # List emails scheduled for future delivery, soonest first.
        #
        # @return [Array<Hash>]
        def list_scheduled
          @transport.request(:get, "/v1/emails/scheduled")
        end

        # Cancel a scheduled email.
        #
        # @param id [String]
        # @return [Hash] { id:, status: }
        def cancel_scheduled(id)
          @transport.request(:delete, "/v1/emails/scheduled/#{escape(id)}")
        end

        # Send a scheduled email immediately instead of waiting.
        #
        # @param id [String]
        # @return [Hash] { id:, status: }
        def send_scheduled_now(id)
          @transport.request(:post, "/v1/emails/scheduled/#{escape(id)}/send-now")
        end

        # Poll for emails whose status changed at or after +since+ (ISO 8601
        # string or a Time). Capped at 50 rows.
        #
        # @param since [String, Time]
        # @return [Array<Hash>]
        def updates(since)
          iso = since.respond_to?(:iso8601) ? since.iso8601 : since
          @transport.request(:get, "/v1/emails/updates", query: { since: iso })
        end

        # Get the caller's saved searches.
        #
        # @return [Array<Hash>]
        def get_saved_searches
          @transport.request(:get, "/v1/emails/saved-searches")[:searches]
        end

        # Replace the caller's saved searches (max 50).
        #
        # @param searches [Array<Hash>]
        # @return [Array<Hash>]
        def set_saved_searches(searches)
          @transport.request(:put, "/v1/emails/saved-searches", body: { searches: searches })[:searches]
        end

        private

        def escape(value)
          Util.escape(value)
        end

        # Build the JSON body for a send request. The API names the sender field
        # +from_+ on the wire; this is the single place that mapping happens.
        def serialize_send(params)
          p = Util.symbolize(params)
          send_at = p[:send_at] || p[:sendAt]
          send_at = send_at.iso8601 if send_at.respond_to?(:iso8601)
          reply_to = p[:reply_to] || p[:replyTo]
          Util.prune(
            from_: Util.address(p[:from]),
            to: Util.address_list(p[:to]),
            subject: p[:subject],
            html: p[:html],
            text: p[:text],
            cc: Util.address_list(p[:cc]),
            bcc: Util.address_list(p[:bcc]),
            reply_to: reply_to.nil? ? nil : Util.address(reply_to),
            headers: p[:headers],
            tags: p[:tags],
            send_at: send_at,
            attachments: p[:attachments]
          )
        end
      end
    end
  end
end
