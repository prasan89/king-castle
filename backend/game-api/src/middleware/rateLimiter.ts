import rateLimit from 'express-rate-limit';

/**
 * General API rate limiter: 60 requests per minute per IP.
 */
export const generalLimiter = rateLimit({
  windowMs: 60 * 1000,
  max: 60,
  standardHeaders: true,
  legacyHeaders: false,
  message: { success: false, error: 'Too many requests, please try again later.' },
});

/**
 * Reward endpoint rate limiter: 10 requests per minute per IP.
 * Applied to /api/v1/rewards/* routes.
 */
export const rewardLimiter = rateLimit({
  windowMs: 60 * 1000,
  max: 10,
  standardHeaders: true,
  legacyHeaders: false,
  message: { success: false, error: 'Too many reward requests, please slow down.' },
});
