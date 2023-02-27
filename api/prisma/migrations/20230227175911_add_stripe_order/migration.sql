-- CreateTable
CREATE TABLE "StripeOrder" (
    "id" SERIAL NOT NULL,
    "sessionId" TEXT NOT NULL,
    "tokenId" INTEGER NOT NULL,
    "checkoutCompleted" BOOLEAN NOT NULL DEFAULT false,
    "checkoutExpired" BOOLEAN NOT NULL DEFAULT false,
    "createdAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,
    "updatedAt" TIMESTAMP(3) NOT NULL DEFAULT CURRENT_TIMESTAMP,

    CONSTRAINT "StripeOrder_pkey" PRIMARY KEY ("id")
);
