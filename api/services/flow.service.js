const fcl = require('@onflow/fcl')
const utils = require('../utils/flow')
const { PrismaClient } = require('@prisma/client')
const createError = require('http-errors')
const prisma = new PrismaClient()

require('dotenv').config()
utils.switchToTestnet()

class flowService {
  static async getAdminAccount() {

    const FlowSigner = (await import('../utils/signer.mjs')).default
    // TODO: encrypt private key
    // TODO: multiple keys for concurrency
    const signer = new FlowSigner(
      process.env.ADMIN_ADDRESS, 
      process.env.ADMIN_ENCRYPTED_PRIVATE_KEY,
      process.env.ADMIN_KEY_INDEX,
      {}
    )
    return signer
  }

  static generateKeypair() {
    const EC = require("elliptic").ec
    const ec = new EC("p256")
    const keypair = ec.genKeyPair()

    const privateKey = keypair.getPrivate().toString('hex')
    const publicKey = keypair.getPublic().encode('hex').substring(2)
    return {privateKey: privateKey, publicKey: publicKey}
  }

  static isGeneratingAccounts = false

  static async generateWonderArenaAccounts() {
    if (this.isGeneratingAccounts) {
      return
    }

    this.isGeneratingAccounts = true
    const users = await prisma.user.findMany({
      where: { flowAccount: null }
    })

    for (let i = 0; i < users.length; i++) {
      let user = users[i]
      try {
        await this.createWonderArenaAccount(user)
      } catch (e) {
        console.log(e)
      }
    }
    this.isGeneratingAccounts = false
  }

  static async createWonderArenaAccount(data) {
    const { name, email } = data
    const user = await prisma.user.findUnique({
      where: { email },
      include: { flowAccount: true } 
    })

    if (!user) {
      throw createHttpError.NotFound('User not found')
    }

    if (user.flowAccount) {
      delete user.flowAccount.id
      delete user.flowAccount.encryptedPrivateKey
      delete user.flowAccount.userId
      return user.flowAccount
    }

    const signer = await this.getAdminAccount()

    const {privateKey: privateKey, publicKey: publicKeyHex} = this.generateKeypair()
    const code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

    transaction(name: String, publicKeyHex: String) {
      prepare(signer: AuthAccount) {
          let publicKey = publicKeyHex.decodeHex()

          let key = PublicKey(
              publicKey: publicKey,
              signatureAlgorithm: SignatureAlgorithm.ECDSA_P256
          )
  
          let account = AuthAccount(payer: signer)
  
          account.keys.add(
              publicKey: key,
              hashAlgorithm: HashAlgorithm.SHA3_256,
              weight: 1000.0
          )

          let player <- WonderArenaBattleField_BasicBeasts1.createNewPlayer(
              name: name,
              address: account.address
          )

          account.save(<- player, to: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
          let playerCap = account.link<&WonderArenaBattleField_BasicBeasts1.Player{WonderArenaBattleField_BasicBeasts1.PlayerPublic}>(
              WonderArenaBattleField_BasicBeasts1.PlayerPublicPath, 
              target: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
              ?? panic("link player failed")

          WonderArenaBattleField_BasicBeasts1.register(playerCap: playerCap)
      }
    }
    `
    const txid = await signer.sendTransaction(code, (arg, t) => [
      arg(name, t.String),
      arg(publicKeyHex, t.String)
    ])

    if (txid) {
      let tx = await fcl.tx(txid).onceSealed()
      console.log(tx)
      let event = tx.events.find((e) => e.type == 'flow.AccountCreated')
      console.log(event)
      if (!event) {
        throw "Account generate failed"
      }
      const address = event.data.address
      let flowAccount = await prisma.flowAccount.create({
          data: {
            address: address,
            encryptedPrivateKey: privateKey,
            userId: user.id
          }
        })

      delete flowAccount.id
      delete flowAccount.encryptedPrivateKey
      delete flowAccount.userId
    
      return flowAccount
    }

    throw "Account generate failed"
  }
}

module.exports = flowService