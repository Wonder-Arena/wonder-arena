const { PrismaClient, Prisma } = require('@prisma/client')
const prisma = new PrismaClient()

require('dotenv').config()
const bcrypt = require('bcryptjs')
const jwt = require('../utils/jwt')
const createError = require('http-errors')

class authService {

    static async registerOrLogin(userData) {
      const { name, email } = userData
      try {
        let user = await prisma.user.findUnique({
          where: { email: email }
        })

        userData.password = email
        if (user) {
          console.log("Login")
          return this.login(userData)
        }
        console.log("Sign up")
        return this.register(userData)
      } catch (e) {
        throw createError.UnprocessableEntity("auth failed")
      }
    }

    static async register(data) {
        data.password = bcrypt.hashSync(data.password, 8)

        try {
          let flowAccount = await prisma.flowAccount.findFirst({
            where: { userId: null }
          })

          let user = null
          if (flowAccount) {
            data.claimedBBs = true
            try {
              let _user = await prisma.$transaction(async (tx) => {
                let user = await tx.user.create({
                  data
                })
  
                await tx.flowAccount.update({
                  where: { address: flowAccount.address },
                  data: { userId: user.id }
                })
  
                return user
              })
              user = _user
            } catch (e) {
              console.log(e)
              throw createError.InternalServerError("register failed")
            }
          } else {
            user = await prisma.user.create({
              data
            }) 
          }

          data.accessToken = await jwt.signAccessToken(user);
        } catch (e) {
          if (e instanceof Prisma.PrismaClientKnownRequestError) {
            // P2022: Unique constraint failed
            // Prisma error codes: https://www.prisma.io/docs/reference/api-reference/error-reference#error-codes
            if (e.code === 'P2002') {
              throw createError.UnprocessableEntity('The email or username already registered')
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
            },
            include: {
              flowAccount: true
            }
        })

        if (!user) {
            throw createError.NotFound('User not registered')
        }

        const checkPassword = bcrypt.compareSync(password, user.password)
        if (!checkPassword) throw createError.Unauthorized('Email address or password not valid')

        if (user.flowAccount) {
          delete user.flowAccount.id
          delete user.flowAccount.encryptedPrivateKey
          delete user.flowAccount.userId
          delete user.flowAccount.createdAt
          delete user.flowAccount.updatedAt
        }

        delete user.password
        delete user.id
        delete user.createdAt
        delete user.updatedAt
        delete user.generatorIndex

        const accessToken = await jwt.signAccessToken(user)

        return { ...user, accessToken }
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