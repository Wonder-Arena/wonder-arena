const router = require('express').Router()
const user = require('../controllers/auth.controller')
const flow = require('../controllers/flow.controller')
const auth = require('../middlewares/auth')

// register
router.post('/', user.register)

// login
router.post('/login', user.login)

// all users
// TODO: get one user
router.get('/', auth, user.all)

// get/create flow account of user
// router.get('/flow/account', auth, flow.account)

router.post('/wonder_arena/add_defender_group', auth, flow.addDefenderGroup)



// fight
// router.post('/fight', auth, flow.fight)

module.exports = router