# frozen_string_literal: true

module Axene
  module Mailer
    module Resources
      # The +contacts+ resource: manage subscriber lists, their contacts, CSV
      # imports, and templated bulk sends. Accessed as +client.contacts+.
      class Contacts
        # @param transport [Axene::Mailer::Transport]
        def initialize(transport)
          @transport = transport
        end

        # List all subscriber lists in the active workspace.
        #
        # @return [Array<Hash>]
        def list_lists
          @transport.request(:get, "/v1/contacts/")
        end

        # Create a subscriber list.
        #
        # @param name [String]
        # @param description [String, nil]
        # @param icon_seed [String, nil]
        # @return [Hash]
        def create_list(name:, description: nil, icon_seed: nil)
          @transport.request(:post, "/v1/contacts/",
                             body: Util.prune(name: name, description: description, icon_seed: icon_seed))
        end

        # Get a list with a page of its contacts (zero-based +page+).
        #
        # @param id [String]
        # @param page [Integer] default 0
        # @param limit [Integer] default 50
        # @return [Hash]
        def get_list(id, page: 0, limit: 50)
          @transport.request(:get, "/v1/contacts/#{Util.escape(id)}", query: { page: page, limit: limit })
        end

        # Update a list's name, description, or icon (partial).
        #
        # @param id [String]
        # @param name [String, nil]
        # @param description [String, nil]
        # @param icon_seed [String, nil]
        # @return [Hash]
        def update_list(id, name: nil, description: nil, icon_seed: nil)
          @transport.request(:patch, "/v1/contacts/#{Util.escape(id)}",
                             body: Util.prune(name: name, description: description, icon_seed: icon_seed))
        end

        # Delete a list and all of its contacts.
        #
        # @param id [String]
        # @return [nil]
        def delete_list(id)
          @transport.request(:delete, "/v1/contacts/#{Util.escape(id)}")
        end

        # Add a single contact to a list.
        #
        # @param list_id [String]
        # @param email [String]
        # @param name [String, nil]
        # @param metadata [Hash, nil]
        # @return [Hash]
        def add_contact(list_id, email:, name: nil, metadata: nil)
          @transport.request(:post, "/v1/contacts/#{Util.escape(list_id)}/contacts",
                             body: Util.prune(email: email, name: name, metadata: metadata))
        end

        # Remove a contact from a list.
        #
        # @param list_id [String]
        # @param contact_id [String]
        # @return [nil]
        def remove_contact(list_id, contact_id)
          @transport.request(:delete, "/v1/contacts/#{Util.escape(list_id)}/contacts/#{Util.escape(contact_id)}")
        end

        # Import contacts from a CSV file (header row required). The email column
        # is auto-detected; other columns become contact metadata.
        #
        # +file+ may be raw bytes (a String) or a path to a readable file.
        #
        # @param list_id [String]
        # @param file [String] raw CSV bytes or a file path
        # @param filename [String]
        # @return [Hash] { imported:, skipped:, errors: }
        def upload_csv(list_id, file, filename: "contacts.csv")
          bytes = read_file(file)
          @transport.upload("/v1/contacts/#{Util.escape(list_id)}/upload", bytes, filename)
        end

        # Send a templated email to every contact in a list. The +contact_list_id+
        # is injected automatically from +list_id+. Subject/html/text may use
        # {{email}}, {{name}}, and {{metadata_key}} placeholders.
        #
        # @param list_id [String]
        # @param sender_address_id [String]
        # @param subject [String]
        # @param html [String, nil]
        # @param text [String, nil]
        # @param tags [Array<String>, nil]
        # @return [Hash] { queued:, skipped:, errors: }
        def bulk_send(list_id, sender_address_id:, subject:, html: nil, text: nil, tags: nil)
          body = Util.prune(
            contact_list_id: list_id,
            sender_address_id: sender_address_id,
            subject: subject,
            html: html,
            text: text,
            tags: tags
          )
          @transport.request(:post, "/v1/contacts/#{Util.escape(list_id)}/send", body: body)
        end

        private

        # Accept either raw bytes or a filesystem path. A path is detected only
        # when the string is short, single-line, and names an existing file.
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
