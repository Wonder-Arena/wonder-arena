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
    const keys = (process.env.ADMIN_ENCRYPTED_PRIVATE_KEYS).split(",")
    const keyIndex = Math.floor(Math.random() * keys.length)
    const key = keys[keyIndex]
    const signer = new FlowSigner(
      process.env.ADMIN_ADDRESS, 
      key,
      keyIndex,
      {}
    )
    return signer
  }

  static async getUserSigner(flowAccount) {
    const FlowSigner = (await import('../utils/signer.mjs')).default
    // TODO: encrypt private key
    const signer = new FlowSigner(
      flowAccount.address,
      flowAccount.encryptedPrivateKey,
      0,
      {}
    )

    return signer 
  }

  static generateKeypair() {
    const EC = require("elliptic").ec
    const ec = new EC("p256")
    const keypair = ec.genKeyPair()

    let privateKey = keypair.getPrivate().toString('hex')
    while (privateKey.length != 64) {
      privateKey = keypair.getPrivate().toString('hex')
    }

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
      throw createError.NotFound('User not found')
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
    import FungibleToken from 0x9a0766d93b6608b7
    import NonFungibleToken from 0x631e88ae7f1d7c20
    import MetadataViews from 0x631e88ae7f1d7c20
    import BasicBeasts from 0xfa252d0aa22bf86a
    
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
    
            // Add some FLOW
            let signerVault <- signer
                .borrow<&{FungibleToken.Provider}>(from: /storage/flowTokenVault)!
                .withdraw(amount: 1.0)
    
            account
                .getCapability(/public/flowTokenReceiver)!
                .borrow<&{FungibleToken.Receiver}>()!
                .deposit(from: <-signerVault)
    
            // setup WonderArena player
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
    
            // setup BasicBeasts collection
            account.save(<-BasicBeasts.createEmptyCollection(), to: BasicBeasts.CollectionStoragePath)
            account.unlink(BasicBeasts.CollectionPublicPath)
            account.link<&BasicBeasts.Collection{NonFungibleToken.Receiver, NonFungibleToken.CollectionPublic, MetadataViews.ResolverCollection, BasicBeasts.BeastCollectionPublic}>(BasicBeasts.CollectionPublicPath, target: BasicBeasts.CollectionStoragePath)
        }
    }
    `
    const txid = await signer.sendTransaction(code, (arg, t) => [
      arg(name, t.String),
      arg(publicKeyHex, t.String)
    ])

    if (txid) {
      let tx = await fcl.tx(txid).onceSealed()
      let event = tx.events.find((e) => e.type == 'flow.AccountCreated')
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

  static async addDefenderGroup(userData, beastIDs) {
    const { name, email } = userData
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

    let signer = await this.getUserSigner(user.flowAccount)
    let code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

    transaction(beastIDs: [UInt64]) {
        let playerRef: &WonderArenaBattleField_BasicBeasts1.Player

        prepare(acct: AuthAccount) {
            self.playerRef = acct.borrow<&WonderArenaBattleField_BasicBeasts1.Player>(from: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
                ?? panic("borrow player failed")
        }

        execute {
            self.playerRef.addDefenderGroup(members: beastIDs)
        }
    }
    `

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(beastIDs.map((id) => id.toString()), t.Array(t.UInt64))
      ])

      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        if (tx.status === 4 && tx.statusCode === 0) {
          return
        }
      }
    } catch (e) {
      console.log(e)
      throw "Add defender group failed"
    }

    throw "Add defender group failed"
  }

  static async removeDefenderGroup(userData, beastIDs) {
    const { name, email } = userData
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

    let signer = await this.getUserSigner(user.flowAccount)
    let code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

    transaction(beastIDs: [UInt64]) {
        let playerRef: &WonderArenaBattleField_BasicBeasts1.Player
    
        prepare(acct: AuthAccount) {
            self.playerRef = acct.borrow<&WonderArenaBattleField_BasicBeasts1.Player>(from: WonderArenaBattleField_BasicBeasts1.PlayerStoragePath)
                ?? panic("borrow player failed")
        }
    
        execute {
            self.playerRef.removeDefenderGroup(members: beastIDs)
        }
    }
    `

    try {
      const txid = await signer.sendTransaction(code, (arg, t) => [
        arg(beastIDs.map((id) => id.toString()), t.Array(t.UInt64))
      ])
  
      if (txid) {
        let tx = await fcl.tx(txid).onceSealed()
        if (tx.status === 4 && tx.statusCode === 0) {
          return
        }
      }
    } catch (e) {
      console.log(e)
      throw "remove defender group failed"
    }


    throw "remove defender group failed"
  }

  static async claimBBs(userData) {
    const { name, email } = userData
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

    if (user.claimedBBs) {
      throw createError.UnprocessableEntity('already claimed')
    }

    let signer = await this.getAdminAccount()
    let code = `
    import NonFungibleToken from 0x631e88ae7f1d7c20
    import BasicBeasts from 0xfa252d0aa22bf86a
    
    transaction(recipient: Address) {
        let senderCollection: &BasicBeasts.Collection
        let recipientCollection: &BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}
    
        prepare(acct: AuthAccount) {
            self.senderCollection = acct.borrow<&BasicBeasts.Collection>(from: BasicBeasts.CollectionStoragePath)
                ?? panic("borrow sender collection failed")
    
            self.recipientCollection = getAccount(recipient)
                .getCapability(BasicBeasts.CollectionPublicPath)
                .borrow<&BasicBeasts.Collection{BasicBeasts.BeastCollectionPublic}>()
                ?? panic("borrow recipient collection failed")
        }
    
        execute {
          let ids = self.senderCollection.getIDs()
          assert(ids.length >= 3, message: "Basic Beasts are not enough")
          let tokenIDs = [ids[0], ids[1], ids[2]]

          for id in tokenIDs {
              let beast <- self.senderCollection.withdraw(withdrawID: id)
              self.recipientCollection.deposit(token: <- beast)
          }
        }
    }
    `

    const txid = await signer.sendTransaction(code, (arg, t) => [
      arg(user.flowAccount.address, t.Address)
    ])

    if (txid) {
      let tx = await fcl.tx(txid).onceSealed()
      if (tx.status === 4 && tx.statusCode === 0) {
        let updatedUser = await prisma.user.update({
          where: { email },
          data: { claimedBBs: true }
        }) 
        return updatedUser
      }
    }

    throw "claim failed"
  }

  static async fight(userData, attackerIDs, defenderAddress) {
    const { name, email } = userData
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

    const defenderAccount = await prisma.flowAccount.findUnique({
      where: { address: defenderAddress },
      include: { user: true }
    })

    if (!defenderAccount.user) {
      throw createError.NotFound('defender not found')
    }

    let signer = await this.getAdminAccount()
    let code = `
    import WonderArenaBattleField_BasicBeasts1 from 0xbca26f5091cd39ec

    transaction(
        attackerAddress: Address,
        attackerIDs: [UInt64],
        defenderAddress: Address
    ) {
        prepare(acct: AuthAccount) {}
    
        execute {
            WonderArenaBattleField_BasicBeasts1.fight(
                attackerAddress: attackerAddress,
                attackerIDs: attackerIDs,
                defenderAddress: defenderAddress
            )
        }
    }
    `

    const txid = await signer.sendTransaction(code, (arg, t) => [
      arg(user.flowAccount.address, t.Address),
      arg(attackerIDs.map((id) => id.toString()), t.Array(t.UInt64)),
      arg(defenderAddress, t.Address)
    ])

    if (txid) {
      let tx = await fcl.tx(txid).onceSealed()
      if (tx.status === 4 && tx.statusCode === 0) {
        return
      }
    }

    throw "fight failed"
  }
}



module.exports = flowService