const { PrismaClient, Prisma } = require('@prisma/client')
const prisma = new PrismaClient()

require('dotenv').config()
const bcrypt = require('bcryptjs')
const jwt = require('../utils/jwt')
const createError = require('http-errors')

class authService {

    static async register(data) {
        data.password = bcrypt.hashSync(data.password, 8)

        try {
          let user = await prisma.user.create({
            data
          })
          data.accessToken = await jwt.signAccessToken(user);
        } catch (e) {
          if (e instanceof Prisma.PrismaClientKnownRequestError) {
            // P2022: Unique constraint failed
            // Prisma error codes: https://www.prisma.io/docs/reference/api-reference/error-reference#error-codes
            if (e.code === 'P2002') {
              throw {statusCode: 401, message: 'The user already exists'}
            }
          }
          throw e
        }

        delete data.password
        
        return data
    }

    static async login(data) {

        const { email, password } = data;
        const user = await prisma.user.findUnique({
            where: {
                email
            }
        })

        if (!user) {
            throw createError.NotFound('User not registered')
        }

        const checkPassword = bcrypt.compareSync(password, user.password)
        if (!checkPassword) throw createError.Unauthorized('Email address or password not valid')

        delete user.password

        const accessToken = await jwt.signAccessToken(user)

        return { ...user, accessToken }
    }

    static async info(name) {
      const user = await prisma.user.findUnique({
        where: {
            name 
        },
        include: {
          flowAccount: true
        }
      })

      if (!user) {
        throw createError.NotFound('User not exists') 
      }

      if (user.flowAccount) {
        delete user.flowAccount.id
        delete user.flowAccount.encryptedPrivateKey
        delete user.flowAccount.userId
      }

      delete user.password
      delete user.id

      return user
    }

    static async all() {

        const allUsers = (await prisma.user.findMany()).map((user) => {
          return this.exclude(user, ["password"])
        })

        return allUsers;
    }

    static exclude(user, keys) {
      for (let key of keys) {
        delete user[key]
      }
      return user
    }
    
}

module.exports = authService;