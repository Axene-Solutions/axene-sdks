# frozen_string_literal: true

module Axene
  module Mailer
    module Resources
      # The +domains+ resource: register, verify, inspect, and transfer sending
      # domains. Accessed as +client.domains+.
      class Domains
        # @param transport [Axene::Mailer::Transport]
        def initialize(transport)
          @transport = transport
        end

        # List your sending domains and their verification status.
        #
        # @return [Array<Hash>]
        def list
          @transport.request(:get, "/v1/domains/")
        end

        # Register a new sending domain. Returns the DNS records to publish.
        #
        # @param name [String]
        # @return [Hash]
        def create(name)
          @transport.request(:post, "/v1/domains/", body: { name: name })
        end

        # Fetch a domain with its DKIM selector and DNS records.
        #
        # @param id [String]
        # @return [Hash]
        def get(id)
          @transport.request(:get, "/v1/domains/#{Util.escape(id)}")
        end

        # Delete a domain.
        #
        # @param id [String]
        # @return [nil]
        def delete(id)
          @transport.request(:delete, "/v1/domains/#{Util.escape(id)}")
        end

        # Re-check DNS and verify the domain.
        #
        # @param id [String]
        # @return [Hash]
        def verify(id)
          @transport.request(:post, "/v1/domains/#{Util.escape(id)}/verify")
        end

        # Run live DNS health checks (DKIM, SPF, DMARC, return-path, MX).
        #
        # @param id [String]
        # @return [Hash]
        def health(id)
          @transport.request(:get, "/v1/domains/#{Util.escape(id)}/health")
        end

        # Diagnose configuration issues and get a health score.
        #
        # @param id [String]
        # @return [Hash]
        def diagnose(id)
          @transport.request(:get, "/v1/domains/#{Util.escape(id)}/diagnose")
        end

        # Current MX status for inbound/forwarding (shape varies by provider).
        #
        # @param id [String]
        # @return [Hash]
        def mx_status(id)
          @transport.request(:get, "/v1/domains/#{Util.escape(id)}/mx-status")
        end

        # The values currently published in DNS for each of the domain's records.
        #
        # @param id [String]
        # @return [Hash]
        def published_records(id)
          @transport.request(:get, "/v1/domains/#{Util.escape(id)}/published-records")
        end

        # Rotate the domain's DKIM key, returning the new record to publish.
        #
        # @param id [String]
        # @return [Hash]
        def rotate_dkim(id)
          @transport.request(:post, "/v1/domains/#{Util.escape(id)}/rotate-dkim")
        end

        # Initiate a transfer of this domain to another Axene account.
        #
        # @param id [String]
        # @param target_email [String]
        # @param note [String, nil]
        # @return [Hash]
        def transfer(id, target_email:, note: nil)
          @transport.request(:post, "/v1/domains/#{Util.escape(id)}/transfer",
                             body: { target_email: target_email, note: note })
        end

        # Check whether a domain name is available to add (checks public DNS).
        #
        # @param name [String]
        # @return [Hash]
        def check_availability(name)
          @transport.request(:get, "/v1/domains/check-availability", query: { name: name })
        end

        # Check whether a domain name already exists in your account.
        #
        # @param name [String]
        # @return [Hash]
        def check(name)
          @transport.request(:get, "/v1/domains/check/#{Util.escape(name)}")
        end
      end
    end
  end
end
