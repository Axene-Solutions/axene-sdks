import { Axene } from '@axene/mailer';

const axene = new Axene({ apiKey: process.env.AXENE_API_KEY! });

const res = await axene.emails.send({
  from: { email: 'hello@yourdomain.com', name: 'Your Shop' },
  to: 'customer@example.com',
  subject: 'Your receipt',
  html: '<h1>Thank you</h1><p>Your order is confirmed.</p>',
  text: 'Thank you. Your order is confirmed.',
  tags: ['receipt'],
});

console.log('queued:', res.id, res.status);
