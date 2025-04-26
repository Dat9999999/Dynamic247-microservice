require("dotenv").config();
const express = require("express");
const { createProxyMiddleware } = require("http-proxy-middleware");
const cors = require("cors");

const app = express();
app.use(cors());

// Proxy đến AuthService
app.use(
  "/auth",
  createProxyMiddleware({
    target: process.env.AUTH_SERVICE_URL,
    changeOrigin: true
  })
);

// Proxy đến ArticleService
app.use(
  "/article",
  createProxyMiddleware({
    target: process.env.ARTICLE_SERVICE_URL,
    changeOrigin: true
  })
);


// Chạy API Gateway
const PORT = process.env.PORT || 8000;
app.listen(PORT, () => {
  console.log(`API Gateway is running on port ${PORT}`);
});
