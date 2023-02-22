const router = require('express').Router()
const user = require('../controllers/auth.controller')
const flow = require('../controllers/flow.controller')
const auth = require('../middlewares/auth')

// register
router.post('/', user.register)

// login
router.post('/login', user.login)

// get user info
// router.get('/', auth, user.info)

router.post('/wonder_arena/get_bbs', auth, flow.claimBBs)
router.post('/wonder_arena/add_defender_group', auth, flow.addDefenderGroup)
router.post('/wonder_arena/remove_defender_group', auth, flow.removeDefenderGroup)
router.post('/wonder_arena/fight', auth, flow.fight)

module.exports = router