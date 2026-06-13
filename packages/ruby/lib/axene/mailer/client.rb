# frozen_string_literal: true

require_relative "transport"
require_relative "resources/emails"
require_relative "resources/domains"
require_relative "resources/contacts"
require_relative "resources/suppressions"
require_relative "resources/templates"
require_relative "resources/webhooks"

module Axene
  module Mailer
    # Axene Mailer API client. Composes the HTTP transport with the resource
    # groups. This is the entry point most code touches.
    #
    # @example
    #   client = Axene::Mailer::Client.new(api_key: ENV.fetch("AXENE_API_KEY"))
    #   client.emails.send(
    #     from: "hello@yourdomain.com",
    #     to: "customer@example.com",
    #     subject: "Your receipt",
    #     html: "<p>Thanks for your order.</p>"
    #   )
    class Client
      # @return [Axene::Mailer::Resources::Emails] send, search, schedule, inspect emails
      attr_reader :emails
      # @return [Axene::Mailer::Resources::Domains] register, verify, transfer domains
      attr_reader :domains
      # @return [Axene::Mailer::Resources::Contacts] manage lists and bulk sends
      attr_reader :contacts
      # @return [Axene::Mailer::Resources::Suppressions] manage the do-not-send list
      attr_reader :suppressions
      # @return [Axene::Mailer::Resources::Templates] manage reusable templates
      attr_reader :templates
      # @return [Axene::Mailer::Resources::Webhooks] manage webhooks, inspect deliveries
      attr_reader :webhooks

      # @param api_key [String] required; starts with "axm_k_"
      # @param base_url [String] default "https://mail.axene.io"
      # @param max_retries [Integer] retries for 429/5xx, default 3
      # @param timeout [Numeric] per-request timeout in seconds, default 30
      def initialize(api_key:, base_url: Transport::DEFAULT_BASE_URL, max_retries: 3, timeout: 30)
        transport = Transport.new(api_key: api_key, base_url: base_url, max_retries: max_retries, timeout: timeout)
        @emails = Resources::Emails.new(transport)
        @domains = Resources::Domains.new(transport)
        @contacts = Resources::Contacts.new(transport)
        @suppressions = Resources::Suppressions.new(transport)
        @templates = Resources::Templates.new(transport)
        @webhooks = Resources::Webhooks.new(transport)
      end
    end
  end
end
