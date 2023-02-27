const router = require('express').Router()
const express = require('express')

const stripe = require('stripe')(process.env.STRIPE_SK)
router.post('/stripe', express.raw({type: 'application/json'}), (request, response) => {
  const sig = request.headers['stripe-signature'];

  let event;
  const endpointSecret = process.env.STRIPE_HOOK_SK

  try {
    event = stripe.webhooks.constructEvent(request.body, sig, endpointSecret);
  } catch (err) {
    console.log(err)
    response.status(400).send(`Webhook Error: ${err.message}`);
    return;
  }

  switch (event.type) {
    case 'payment_intent.succeeded':
      const paymentIntentSucceeded = event.data.object;
      console.log(paymentIntentSucceeded)
      break;
    default:
      console.log(`Unhandled event type ${event.type}`);
  }

  response.send();
});

module.exports = router