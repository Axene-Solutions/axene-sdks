# frozen_string_literal: true

module Axene
  module Mailer
    # Raised for any non-2xx API response, or for a transport failure that
    # survives all retries.
    #
    # Inspect {#status} and {#code} to branch on specific failures (for example
    # a 422 with code "invalid"). A {#status} of 0 indicates a transport or
    # network failure with no HTTP response.
    class Error < StandardError
      # @return [Integer] HTTP status code (0 for transport failures)
      attr_reader :status

      # @return [String, nil] machine-readable error code from the API body
      attr_reader :code

      # @return [Object, nil] the raw parsed response body, for debugging
      attr_reader :detail

      # @param status [Integer]
      # @param message [String]
      # @param code [String, nil]
      # @param detail [Object, nil]
      def initialize(status, message, code = nil, detail = nil)
        super(message)
        @status = status
        @code = code
        @detail = detail
      end
    end
  end
end
