const router = require('express').Router()
const user = require('../controllers/auth.controller')
const flow = require('../controllers/flow.controller')
const stripe = require('../controllers/stripe.controller')
const auth = require('../middlewares/auth')
const bodyParser = require('body-parser');
const cors = require('cors')

router.use(bodyParser.urlencoded({ extended: true }))
router.use(bodyParser.json())

// register
router.post('/', user.register)

// login
router.post('/login', user.login)
// app.get('/',function(req,res) {
  // res.sendFile('index.html');
// });
router.get('/google_auth', cors(), function (req, res) {
  res.sendFile(__dirname + '/index.html')
})

router.post('/google_login', user.googleLogin)

router.post('/flow/account_link', auth, flow.accountLink)

router.get('/wonder_arena/players/:name', auth, flow.getPlayer)

router.post('/wonder_arena/add_defender_group', auth, flow.addDefenderGroup)
router.post('/wonder_arena/remove_defender_group', auth, flow.removeDefenderGroup)
router.post('/wonder_arena/fight', auth, flow.fight)
router.post('/wonder_arena/claim_reward', auth, flow.claimReward)

router.post('/stripe/create_checkout_session', auth, cors(), stripe.createCheckoutSession)

module.exports = router