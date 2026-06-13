# frozen_string_literal: true

require_relative "lib/axene/mailer/version"

Gem::Specification.new do |spec|
  spec.name = "axene-mailer"
  spec.version = Axene::Mailer::VERSION
  spec.authors = ["Axene Solutions"]
  spec.email = ["engineering@axene.io"]

  spec.summary = "Ruby SDK for the Axene Mailer API."
  spec.description = "Send email, manage domains, contacts, suppressions, templates, " \
                     "and webhooks through the Axene Mailer API. Zero runtime dependencies."
  spec.homepage = "https://axene.io"
  spec.license = "MIT"
  spec.required_ruby_version = ">= 3.0"

  spec.metadata = {
    "homepage_uri" => spec.homepage,
    "source_code_uri" => "https://github.com/axene-io/axene-sdks",
    "documentation_uri" => "https://docs.axene.io",
    "rubygems_mfa_required" => "true"
  }

  spec.files = Dir["lib/**/*.rb"] + ["README.md", "LICENSE"]
  spec.require_paths = ["lib"]

  # Zero runtime dependencies: the SDK uses only the Ruby standard library
  # (net/http, json, securerandom, uri).

  spec.add_development_dependency "minitest", "~> 5.0"
  spec.add_development_dependency "rake", "~> 13.0"
end
