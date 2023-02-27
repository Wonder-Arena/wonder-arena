const createError = require('http-errors');
const stripe = require('stripe')(process.env.STRIPE_SK)

class stripeController {
  static createCheckoutSession = async (req, res, next) => {
    try {
      const session = await stripe.checkout.sessions.create({
        line_items: [
          {
            price_data: {
              currency: 'usd',
              product_data: {
                name: 'Basic Beast',
              },
              unit_amount: 1000,
            },
            quantity: 1,
          },
        ],
        mode: 'payment',
        success_url: 'http://localhost:4242/success',
        cancel_url: 'http://localhost:4242/cancel',
      });
    
      res.status(200).json({
        status: true,
        message: null,
        data: {
          sessionID: session.id
        }
      })
    }
    catch (e) {
        console.log(e)
        next(createError(e.statusCode, e.message))
    }
  }
}

module.exports = stripeController