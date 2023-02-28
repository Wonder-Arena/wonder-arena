const { PrismaClient } = require('@prisma/client')
const prisma = new PrismaClient()
const createError = require('http-errors');
const flow = require('./flow.service');
const stripe = require('stripe')(process.env.STRIPE_SK)

class stripeService {
  static async createCheckoutSession(userData, tokenId) {
    const { email } = userData
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true }
    })

    if (!user) {
      throw createError.NotFound('User not found')
    }

    if (!user.flowAccount) {
      throw createError.NotFound('flow account not found')
    }

    const _order = await prisma.stripeOrder.findFirst({
      where: { tokenId: tokenId, checkoutExpired: false } 
    })

    if (_order) {
      throw createError.UnprocessableEntity("has been ordered")
    }

    const session = await this.doCreateSession()
    const data = {
      sessionId: session.id,
      tokenId: tokenId,
      recipient: user.flowAccount.address
    }
    const order = await prisma.stripeOrder.create({
      data
    })

    return {sessionID: session.id, sessionURL: session.url}
  }

  static async handleCheckoutCompleted(sessionId) {
    const order = await prisma.stripeOrder.findUnique({
      where: { sessionId }
    })

    if (order && !order.checkoutCompleted) {
      await prisma.stripeOrder.update({
        where: { sessionId },
        data: { checkoutCompleted: true }
      })
      await flow.sendBeast(order.recipient, order.tokenId)
    }
  }

  static async handleCheckoutExpired(sessionId) {
    const order = await prisma.stripeOrder.findUnique({
      where: { sessionId }
    })

    if (order) {
      await prisma.stripeOrder.update({
        where: { sessionId },
        data: { checkoutExpired: true }
      })
    }
  }

  static async doCreateSession() {
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
      success_url: 'https://www.bakalabs.com/payment-succeeded',
      cancel_url: 'https://www.bakalabs.com/payment-cancelled',
    })
    return session
  }
}

module.exports = stripeService