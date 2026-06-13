# frozen_string_literal: true

module Axene
  module Mailer
    module Resources
      # The +webhooks+ resource: manage event subscriptions and inspect
      # deliveries. Accessed as +client.webhooks+.
      class Webhooks
        # @param transport [Axene::Mailer::Transport]
        def initialize(transport)
          @transport = transport
        end

        # List your active webhooks.
        #
        # @return [Array<Hash>]
        def list
          @transport.request(:get, "/v1/webhooks/")
        end

        # Create a webhook. The signing +secret+ is generated and returned.
        #
        # @param url [String]
        # @param events [Array<String>]
        # @return [Hash]
        def create(url:, events:)
          @transport.request(:post, "/v1/webhooks/", body: { url: url, events: events })
        end

        # Update a webhook's url, events, or active state (partial). The
        # +is_active+ argument maps to the wire field +is_active+.
        #
        # @param id [String]
        # @param url [String, nil]
        # @param events [Array<String>, nil]
        # @param is_active [Boolean, nil]
        # @return [Hash]
        def update(id, url: nil, events: nil, is_active: nil)
          @transport.request(:patch, "/v1/webhooks/#{Util.escape(id)}",
                             body: Util.prune(url: url, events: events, is_active: is_active))
        end

        # Delete a webhook.
        #
        # @param id [String]
        # @return [nil]
        def delete(id)
          @transport.request(:delete, "/v1/webhooks/#{Util.escape(id)}")
        end

        # Queue a sample email.delivered delivery to test the endpoint.
        #
        # @param id [String]
        # @return [Hash] { queued:, url: }
        def test(id)
          @transport.request(:post, "/v1/webhooks/#{Util.escape(id)}/test")
        end

        # List delivery attempts for a webhook (paginated envelope).
        #
        # @param id [String]
        # @param page [Integer] default 0
        # @param limit [Integer] default 20
        # @param status [String, nil]
        # @return [Hash] { items:, total:, page:, limit: }
        def list_deliveries(id, page: 0, limit: 20, status: nil)
          @transport.request(:get, "/v1/webhooks/#{Util.escape(id)}/deliveries",
                             query: { page: page, limit: limit, status: status })
        end

        # Fetch one delivery with its full payload and the endpoint's response.
        #
        # @param id [String]
        # @param delivery_id [String]
        # @return [Hash]
        def get_delivery(id, delivery_id)
          @transport.request(:get, "/v1/webhooks/#{Util.escape(id)}/deliveries/#{Util.escape(delivery_id)}")
        end
      end
    end
  end
end
