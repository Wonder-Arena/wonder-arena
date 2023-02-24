const flow = require('../services/flow.service')
const createError = require('http-errors');
const utils = require('../utils/flow')

class flowController {
    static account = async (req, res, next) => {
        try {
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
            if (!req.body.groupName || !req.body.beastIDs || 
                !Array.isArray(req.body.beastIDs) ||
                req.body.beastIDs.length != 3) {
                    res.status(422).json({
                        status: false,
                        message: "invalid params"
                    })
                return
            }

            await flow.addDefenderGroup(req.user.payload, req.body.groupName, req.body.beastIDs)
            res.status(200).json({
                status: true,
                message: "Defender group added"
            })

        } catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }

    static removeDefenderGroup = async (req, res, next) => {
        try {
            if (!req.body.groupName) {
                res.status(422).json({
                    status: false,
                    message: "invalid params"
                })
                return
            }

            await flow.removeDefenderGroup(req.user.payload, req.body.groupName)
            res.status(200).json({
                status: true,
                message: "Defender group removed"
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
                message: "BBs claimed"
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
                    res.status(422).json({
                        status: false,
                        message: "invalid params"
                    })
                return
            }

            const data = await flow.fight(req.user.payload, req.body.attackerIDs, req.body.defenderAddress)
            res.status(200).json({
                status: true,
                message: "Battle finished",
                data: data
            })

        } catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }

    static claimReward = async (req, res, next) => {
        try {
            await flow.claimReward(req.user.payload)
            res.status(200).json({
                status: true,
                message: ""
            })

        } catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }

    static accountLink = async (req, res, next) => {
        try {
            if (!req.body.parentAddress || !utils.isValidFlowAddress(req.body.parentAddress)) {
                res.status(422).json({
                    status: false,
                    message: "invalid params"
                })
                return
            }

            await flow.accountLink(req.user.payload, req.body.parentAddress)
            res.status(200).json({
                status: true,
                message: ""
            })

        } catch (e) {
            console.log(e)
            next(createError(e.statusCode, e.message))
        }
    }
}

setInterval(async function() {
    try {
        await flow.generateWonderArenaAccounts()
    } catch (e) {
        console.log(e)
    }
}, 6000);

module.exports = flowController