# frozen_string_literal: true

module Axene
  module Mailer
    module Resources
      # The +templates+ resource: reusable email templates (Starter plan and up).
      # Accessed as +client.templates+.
      #
      # Note the wire mapping for this resource: +html+ maps to +html_body+ and
      # +text+ maps to +text_body+ (the emails resource keeps +html+/+text+).
      class Templates
        # @param transport [Axene::Mailer::Transport]
        def initialize(transport)
          @transport = transport
        end

        # List all templates, most recently updated first.
        #
        # @return [Array<Hash>]
        def list
          @transport.request(:get, "/v1/templates/")
        end

        # Create a template. +variables+ are derived server-side from {{name}}
        # placeholders in the bodies, so you do not pass them.
        #
        # @param name [String]
        # @param subject [String, nil]
        # @param html [String, nil] maps to html_body
        # @param text [String, nil] maps to text_body
        # @param blocks_json [Hash, nil]
        # @return [Hash]
        def create(name:, subject: nil, html: nil, text: nil, blocks_json: nil)
          @transport.request(:post, "/v1/templates/", body: serialize(name, subject, html, text, blocks_json))
        end

        # Fetch a single template.
        #
        # @param id [String]
        # @return [Hash]
        def get(id)
          @transport.request(:get, "/v1/templates/#{Util.escape(id)}")
        end

        # Update a template (partial).
        #
        # @param id [String]
        # @param name [String, nil]
        # @param subject [String, nil]
        # @param html [String, nil] maps to html_body
        # @param text [String, nil] maps to text_body
        # @param blocks_json [Hash, nil]
        # @return [Hash]
        def update(id, name: nil, subject: nil, html: nil, text: nil, blocks_json: nil)
          @transport.request(:patch, "/v1/templates/#{Util.escape(id)}",
                             body: serialize(name, subject, html, text, blocks_json))
        end

        # Delete a template.
        #
        # @param id [String]
        # @return [nil]
        def delete(id)
          @transport.request(:delete, "/v1/templates/#{Util.escape(id)}")
        end

        # Duplicate a template (the copy's +blocks_json+ is not carried over).
        #
        # @param id [String]
        # @return [Hash]
        def duplicate(id)
          @transport.request(:post, "/v1/templates/#{Util.escape(id)}/duplicate")
        end

        private

        def serialize(name, subject, html, text, blocks_json)
          Util.prune(
            name: name,
            subject: subject,
            html_body: html,
            text_body: text,
            blocks_json: blocks_json
          )
        end
      end
    end
  end
end
