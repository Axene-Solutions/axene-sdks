# frozen_string_literal: true

require_relative "mailer/version"
require_relative "mailer/error"
require_relative "mailer/util"
require_relative "mailer/transport"
require_relative "mailer/client"

# Axene Solutions namespace.
module Axene
  # Ruby SDK for the Axene Mailer API (https://mail.axene.io).
  #
  # @example
  #   require "axene/mailer"
  #   client = Axene::Mailer::Client.new(api_key: ENV.fetch("AXENE_API_KEY"))
  #   client.emails.send(from: "hi@you.io", to: "x@example.com", subject: "Hi")
  module Mailer
  end
end
