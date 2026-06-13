# frozen_string_literal: true

module Axene
  module Mailer
    module Resources
      # The +suppressions+ resource: manage the do-not-send list. Accessed as
      # +client.suppressions+.
      class Suppressions
        # @param transport [Axene::Mailer::Transport]
        def initialize(transport)
          @transport = transport
        end

        # List suppressed addresses (paginated envelope; zero-based +page+).
        #
        # @param page [Integer] default 0
        # @param limit [Integer] default 50
        # @param search [String, nil]
        # @return [Hash] { items:, total:, page:, limit: }
        def list(page: 0, limit: 50, search: nil)
          @transport.request(:get, "/v1/suppressions", query: { page: page, limit: limit, search: search })
        end

        # Suppress a single address. The +email+ argument maps to the wire field
        # +email_address+.
        #
        # @param email [String]
        # @param reason [String] default "manual"
        # @return [Hash]
        def add(email:, reason: "manual")
          @transport.request(:post, "/v1/suppressions", body: { email_address: email, reason: reason })
        end

        # Bulk-import suppressions from a file (one email per line). +file+ may be
        # raw bytes (a String) or a path to a readable file.
        #
        # @param file [String] raw bytes or a file path
        # @param filename [String]
        # @return [Hash] { added:, skipped:, total_processed: }
        def bulk_upload(file, filename: "suppressions.txt")
          bytes = read_file(file)
          @transport.upload("/v1/suppressions/bulk", bytes, filename)
        end

        # Remove an address from the suppression list.
        #
        # @param id [String]
        # @return [nil]
        def remove(id)
          @transport.request(:delete, "/v1/suppressions/#{Util.escape(id)}")
        end

        private

        def read_file(file)
          if file.is_a?(String) && file.length < 4096 && !file.include?("\n") && File.file?(file)
            File.binread(file)
          else
            file
          end
        end
      end
    end
  end
end
