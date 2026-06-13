# Axene Mailer PHP SDK

Official PHP client for the [Axene Mailer](https://mail.axene.io) API. Email
marketing and transactional email for Africa, with KES pricing and M-Pesa
billing.

## Requirements

- PHP 8.1+
- [Guzzle](https://docs.guzzlephp.org/) 7.5+ (installed as a dependency)

## Install

```bash
composer require axene/mailer
```

## Quickstart

```php
<?php

require 'vendor/autoload.php';

use Axene\Mailer\Client;
use Axene\Mailer\AxeneException;

$axene = new Client(getenv('AXENE_API_KEY')); // key starts with axm_k_

try {
    $result = $axene->emails->send([
        'from' => 'hello@yourdomain.com',
        'to' => 'customer@example.com',
        'subject' => 'Your receipt',
        'html' => '<p>Thanks for your order.</p>',
    ]);

    echo $result['id'];
} catch (AxeneException $e) {
    // $e->getStatus() is the HTTP status; $e->getCode() is the API error code.
    fwrite(STDERR, $e->getStatus() . ' ' . $e->getMessage() . "\n");
}
```

### Configuration

```php
$axene = new Client('axm_k_...', [
    'baseUrl' => 'https://mail.axene.io', // default
    'maxRetries' => 3,                    // 429/5xx retries with backoff
    'timeout' => 30,                      // seconds
]);
```

## Addresses

Anywhere an address is expected (`from`, `to`, `cc`, `bcc`, `replyTo`) you can
pass either a bare string or an array. A string is sugar for `['email' => ...]`.

```php
$axene->emails->send([
    'from' => ['email' => 'hello@yourdomain.com', 'name' => 'Acme'],
    'to' => ['a@example.com', ['email' => 'b@example.com', 'name' => 'B']],
    'subject' => 'Hi',
    'text' => 'Hello there',
]);
```

## Resources

The client exposes six resource groups. Methods return associative arrays
(decoded JSON). List endpoints return either a bare array or a paginated
envelope `['items' => ..., 'total' => ..., 'page' => ..., 'limit' => ...]`,
matching the API. Pagination is **zero-based** (`page = 0` is the first page).

### Emails

```php
$axene->emails->send($message);
$axene->emails->sendBatch([$messageA, $messageB]);
$axene->emails->validate($message);          // dry run, never sends
$axene->emails->list(['status' => 'sent', 'page' => 0, 'limit' => 20]);
$axene->emails->get($id);
$axene->emails->events($id);
$axene->emails->retry($id);
$axene->emails->search(['q' => 'to:user@x.io status:bounced']);
$axene->emails->listScheduled();
$axene->emails->cancelScheduled($id);
$axene->emails->sendScheduledNow($id);
$axene->emails->updates('2026-06-13T00:00:00Z'); // since (required)
$axene->emails->getSavedSearches();
$axene->emails->setSavedSearches($searches);
```

### Domains

```php
$axene->domains->list();
$axene->domains->create('mail.yourdomain.com');
$axene->domains->get($id);
$axene->domains->delete($id);
$axene->domains->verify($id);
$axene->domains->health($id);
$axene->domains->diagnose($id);
$axene->domains->mxStatus($id);
$axene->domains->publishedRecords($id);
$axene->domains->rotateDkim($id);
$axene->domains->transfer($id, ['targetEmail' => 'new@owner.io', 'note' => 'handover']);
$axene->domains->checkAvailability('yourdomain.com');
$axene->domains->check('yourdomain.com');
```

> Advanced domain features (NS provider, BIMI, Domain Connect) are not yet
> covered by this SDK.

### Contacts

```php
$axene->contacts->listLists();
$axene->contacts->createList(['name' => 'Newsletter', 'iconSeed' => 'abc']);
$axene->contacts->getList($listId, ['page' => 0, 'limit' => 50]);
$axene->contacts->updateList($listId, ['name' => 'Renamed']);
$axene->contacts->deleteList($listId);
$axene->contacts->addContact($listId, ['email' => 'a@x.io', 'name' => 'A']);
$axene->contacts->removeContact($listId, $contactId);
$axene->contacts->uploadCsv($listId, file_get_contents('contacts.csv'), 'contacts.csv');
$axene->contacts->bulkSend($listId, [
    'senderAddressId' => 'sa_1',
    'subject' => 'Hello {{name}}',
    'html' => '<p>Hi {{name}}</p>',
]);
```

### Suppressions

```php
$axene->suppressions->list(['page' => 0, 'limit' => 50, 'search' => 'x.io']);
$axene->suppressions->add(['email' => 'bounce@x.io', 'reason' => 'manual']);
$axene->suppressions->bulkUpload(file_get_contents('list.txt'), 'list.txt');
$axene->suppressions->remove($id);
```

### Templates

```php
$axene->templates->list();
$axene->templates->create(['name' => 'Welcome', 'html' => '<p>{{name}}</p>']);
$axene->templates->get($id);
$axene->templates->update($id, ['subject' => 'New subject']);
$axene->templates->delete($id);
$axene->templates->duplicate($id);
```

### Webhooks

```php
$axene->webhooks->list();
$axene->webhooks->create(['url' => 'https://x.io/hook', 'events' => ['email.delivered']]);
$axene->webhooks->update($id, ['isActive' => false]);
$axene->webhooks->delete($id);
$axene->webhooks->test($id);
$axene->webhooks->listDeliveries($id, ['page' => 0, 'limit' => 20]);
$axene->webhooks->getDelivery($id, $deliveryId);
```

## Errors

Every non-2xx response (and any transport failure that survives retries) throws
`Axene\Mailer\AxeneException`:

- `getStatus(): int` the HTTP status (`0` for a network failure)
- `getCode(): ?string` the machine-readable API error code, when present
- `getMessage(): string` the human-readable message
- `getDetail(): mixed` the raw decoded response body

Requests are retried automatically on `429` and `5xx` with exponential backoff,
honouring the `Retry-After` header. `4xx` errors are never retried.

## Development

```bash
composer install
./vendor/bin/phpunit
```

## License

MIT
