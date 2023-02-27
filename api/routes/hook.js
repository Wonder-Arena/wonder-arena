const router = require('express').Router()
const express = require('express')
const { handleCheckoutCompleted, handleCheckoutExpired } = require('../services/stripe.service')

const stripe = require('stripe')(process.env.STRIPE_SK)

router.post('/stripe', express.raw({type: 'application/json'}), async (request, response) => {
  const sig = request.headers['stripe-signature'];

  let event;
  const endpointSecret = process.env.STRIPE_HOOK_SK

  try {
    event = stripe.webhooks.constructEvent(request.body, sig, endpointSecret);
  } catch (err) {
    response.status(400).send(`Webhook Error: ${err.message}`);
    return;
  }

  switch (event.type) {
    case 'checkout.session.completed':
      const checkoutSessionCompleted = event.data.object;
      const completedId = checkoutSessionCompleted.id
      await handleCheckoutCompleted(completedId)
      break;
    case 'checkout.session.expired':
      const checkoutSessionExpired = event.data.object;
      const expiredId = checkoutSessionExpired.id
      await handleCheckoutExpired(expiredId)
    default:
      console.log(`Unhandled event type ${event.type}`);
  }

  response.send();
});

module.exports = router