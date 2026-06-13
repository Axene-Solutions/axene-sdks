# frozen_string_literal: true

require "uri"

module Axene
  module Mailer
    # Internal helpers that translate the SDK's ergonomic inputs into the exact
    # JSON shape the API expects. Not part of the public API.
    module Util
      module_function

      # Drop keys whose value is nil so they are omitted from the JSON body.
      #
      # @param hash [Hash]
      # @return [Hash]
      def prune(hash)
        hash.reject { |_, v| v.nil? }
      end

      # Normalize a single address. A bare String becomes { email: ... }.
      #
      # @param addr [String, Hash, nil]
      # @return [Hash, nil]
      def address(addr)
        return nil if addr.nil?
        return { email: addr } if addr.is_a?(String)

        symbolize(addr)
      end

      # Normalize one-or-many addresses into an array, or nil if absent.
      #
      # @param addr [String, Hash, Array, nil]
      # @return [Array<Hash>, nil]
      def address_list(addr)
        return nil if addr.nil?

        list = addr.is_a?(Array) ? addr : [addr]
        list.map { |a| address(a) }
      end

      # Shallow-symbolize the keys of a Hash so callers may pass either string
      # or symbol keys.
      #
      # @param hash [Hash]
      # @return [Hash]
      def symbolize(hash)
        return hash unless hash.is_a?(Hash)

        hash.each_with_object({}) { |(k, v), acc| acc[k.to_sym] = v }
      end

      # URL-escape a path segment.
      #
      # @param value [#to_s]
      # @return [String]
      def escape(value)
        URI.encode_www_form_component(value.to_s)
      end
    end
  end
end
