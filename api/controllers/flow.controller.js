const flow = require('../services/flow.service')
const createError = require('http-errors');

class flowController {
    static account = async (req, res, next) => {
        try {
            console.log(req.user)
            let account = await flow.createWonderArenaAccount(req.user.payload)
            res.status(200).json({
                status: true,
                message: "Flow account is created",
                data: account
            })
        } catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }

    static addDefenderGroup = async (req, res, next) => {
        try {
            if (!req.body.beastIDs || 
                !Array.isArray(req.body.beastIDs) ||
                req.body.beastIDs.length != 3) {
                    res.status(401).json({
                        status: false,
                        message: "invalid params",
                        data: {}
                    })
                return
            }

            await flow.addDefenderGroup(req.user.payload, req.body.beastIDs)
            res.status(200).json({
                status: true,
                message: "Defender group added",
                data: {}
            })

        } catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }

    static removeDefenderGroup = async (req, res, next) => {
        try {
            if (!req.body.beastIDs || 
                !Array.isArray(req.body.beastIDs) ||
                req.body.beastIDs.length != 3) {
                    res.status(401).json({
                        status: false,
                        message: "invalid params",
                        data: {}
                    })
                return
            }

            await flow.removeDefenderGroup(req.user.payload, req.body.beastIDs)
            res.status(200).json({
                status: true,
                message: "Defender group removed",
                data: {}
            })

        } catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }

    static claimBBs = async (req, res, next) => {
        try {
            await flow.claimBBs(req.user.payload)
            res.status(200).json({
                status: true,
                message: "BBs claimed",
                data: {}
            })

        } catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }

    static fight = async (req, res, next) => {
        try {
            if (!req.body.attackerIDs || 
                !Array.isArray(req.body.attackerIDs) ||
                req.body.attackerIDs.length != 3 ||
                !req.body.defenderAddress) {
                    res.status(401).json({
                        status: false,
                        message: "invalid params",
                        data: {}
                    })
                return
            }

            await flow.fight(req.user.payload, req.body.attackerIDs, req.body.defenderAddress)
            res.status(200).json({
                status: true,
                message: "Battle finished",
                data: {}
            })

        } catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }
}

setInterval(async function() {
    await flow.generateWonderArenaAccounts()
}, 6000);

module.exports = flowController