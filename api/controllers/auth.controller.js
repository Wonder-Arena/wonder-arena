const auth = require('../services/auth.service');
const createError = require('http-errors');

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

    static info = async (req, res, next) => {
      try {
        const user = await auth.info(req.params.name)
        res.status(200).json({
          status: true,
          message: "",
          data: user
        })
      } catch (e) {
        console.log(e)
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