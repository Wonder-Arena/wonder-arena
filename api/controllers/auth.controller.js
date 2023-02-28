const auth = require('../services/auth.service');
const createError = require('http-errors');
const {OAuth2Client} = require('google-auth-library');

class authController {

    static validateEmail = (email) => {
      return String(email)
        .toLowerCase()
        .match(
          /^(([^<>()[\]\\.,;:\s@"]+(\.[^<>()[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/
        );
    }

    static register = async (req, res, next) => {

        try {
            const { name, email, password } = req.body
            if (!name || !email || !password || !this.validateEmail(email)) {
              throw {statusCode: 422, message: "invalid params"}
            }

            const user = await auth.register(req.body)
            res.status(200).json({
                status: true,
                message: 'User created successfully',
                data: user
            })

        }
        catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }

    static googleLogin = async (req, res, next) => {
      const CLIENT_ID = process.env.GOOGLE_CLIENT_ID 
      const client = new OAuth2Client(CLIENT_ID);
      try {
        const ticket = await client.verifyIdToken({
          idToken: req.body.credential,
          audience: CLIENT_ID, 
        });
        const payload = ticket.getPayload()
        const name = payload.name
        const email = payload.email
        const data = await auth.registerOrLogin({name: name, email: email})
        res.status(200).json({
          status: true,
          message: "Account login successful",
          data
        }) 
      } catch (e) {
        console.log(e)
        next(createError(e.statusCode, e.message))
      }
    }

    static login = async (req, res, next) => {

         try {
            const data = await auth.login(req.body)
            res.status(200).json({
                status: true,
                message: "Account login successful",
                data
            })
        } catch (e) {
            next(createError(e.statusCode, e.message))
        }
    }

    static all = async (req, res, next) => {
        try {
            const users = await auth.all();
            res.status(200).json({
                status: true,
                message: 'All users',
                data: users
            })
        } catch (e) {
            next(createError(e.statusCode, e.message))
        }
    }
}

module.exports = authController