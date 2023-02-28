const createError = require('http-errors');
const stripe = require('../services/stripe.service')

class stripeController {
  static createCheckoutSession = async (req, res, next) => {
    try {
      const tokenId = req.body.tokenId
      if (!tokenId) {
        res.status(422).json({
          status: false,
          message: "invalid params"
        })
        return
      }

      const data = await stripe.createCheckoutSession(req.user.payload, tokenId)
      res.status(200).json({
        status: true,
        message: "",
        data: data
      })
    }
    catch (e) {
        console.log(e)
        next(createError(e.statusCode, e.message))
    }
  }
}

module.exports = stripeController