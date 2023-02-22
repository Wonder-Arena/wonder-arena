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
}

setInterval(async function() {
    console.log("generateWonderArenaAccounts")
    await flow.generateWonderArenaAccounts()
}, 6000);

module.exports = flowController