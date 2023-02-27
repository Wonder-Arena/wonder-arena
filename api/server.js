const express = require('express');
require('@prisma/client');
const app = express();
require('dotenv').config();
const route = require('./routes');

const morgan = require("morgan");

app.use(morgan('dev'));

// redirect to routes/index.js
app.use('/', route)

const port = process.env.PORT || 5000;
app.listen(port, () => {
    console.log(`server is running on port ${port}`);
});