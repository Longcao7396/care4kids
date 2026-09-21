/**
 * stripeService.js — Lazy-loads Stripe.js and wraps card payment confirmation.
 *
 * Usage:
 *   import { loadStripe, confirmCardPayment } from './stripeService';
 *
 *   const stripe = await loadStripe(publishableKey);
 *   const result = await confirmCardPayment(stripe, clientSecret, cardElement, {
 *     payment_method_data: { billing_details: { name: 'Nguyen Van A' } }
 *   });
 */

const STRIPE_JS_URL = 'https://js.stripe.com/v3/';
let _stripeScriptLoaded = false;
let _stripeScriptLoading = false;
let _loadStripePromise = null;

/**
 * Loads the Stripe.js script lazily (singleton).
 * Resolves with the window.Stripe instance.
 * Rejects if the script fails to load.
 */
export function loadStripe() {
  if (window.Stripe) {
    return Promise.resolve(window.Stripe);
  }

  if (_loadStripePromise) {
    return _loadStripePromise;
  }

  _loadStripePromise = new Promise((resolve, reject) => {
    if (_stripeScriptLoaded) {
      resolve(window.Stripe);
      return;
    }

    if (_stripeScriptLoading) {
      // Already in the process — poll until it's ready
      const poll = setInterval(() => {
        if (window.Stripe) {
          clearInterval(poll);
          resolve(window.Stripe);
        }
      }, 100);
      // Safety timeout
      setTimeout(() => {
        clearInterval(poll);
        if (!window.Stripe) reject(new Error('Stripe.js load timeout'));
      }, 15000);
      return;
    }

    _stripeScriptLoading = true;

    const script = document.createElement('script');
    script.src = STRIPE_JS_URL;
    script.async = true;
    script.onload = () => {
      _stripeScriptLoaded = true;
      _stripeScriptLoading = false;
      if (window.Stripe) {
        resolve(window.Stripe);
      } else {
        reject(new Error('Stripe.js loaded but window.Stripe is not defined.'));
      }
    };
    script.onerror = () => {
      _stripeScriptLoading = false;
      reject(new Error('Failed to load Stripe.js from ' + STRIPE_JS_URL));
    };

    document.head.appendChild(script);
  });

  return _loadStripePromise;
}

/**
 * Creates a Stripe Elements card element mounted to a DOM element.
 * Returns { stripe, elements, cardElement } for use in confirmCardPayment.
 *
 * @param {string} publishableKey — pk_test_... or pk_live_...
 * @param {HTMLElement|string} mountTarget — DOM element or CSS selector to mount the card input
 * @param {object} options — Stripe Elements appearance options
 */
export async function createCardElement(publishableKey, mountTarget, options = {}) {
  const stripe = await loadStripe();
  const instance = stripe(publishableKey, {
    locale: 'vi', // Vietnamese-friendly
  });

  const elements = instance.elements({
    fonts: [
      {
        cssSrc: 'https://fonts.googleapis.com/css2?family=Inter:wght@400;500&display=swap',
      },
    ],
  });

  const cardElement = elements.create('card', {
    hidePostalCode: false,
    style: {
      base: {
        fontFamily: '"Inter", "Helvetica Neue", Helvetica, sans-serif',
        fontSize: '16px',
        color: '#32325d',
        '::placeholder': { color: '#aab7c4' },
      },
      invalid: { color: '#fa755a', iconColor: '#fa755a' },
    },
    ...options,
  });

  const target = typeof mountTarget === 'string'
    ? document.querySelector(mountTarget)
    : mountTarget;

  if (target) {
    cardElement.mount(target);
  }

  return { stripe: instance, elements, cardElement };
}

/**
 * Confirms a card payment using the payment intent client secret.
 *
 * @param {object} stripe — Stripe instance from createCardElement()
 * @param {string} clientSecret — clientSecret from POST /api/donations response
 * @param {object} cardElement — Stripe card Element from createCardElement()
 * @param {object} extraData — additional confirmParams (e.g. billing_details)
 * @returns {Promise<{ error?: object, paymentIntent?: object }>}
 */
export async function confirmCardPayment(stripe, clientSecret, cardElement, extraData = {}) {
  const { error, paymentIntent } = await stripe.confirmCardPayment(clientSecret, {
    payment_method: {
      card: cardElement,
      ...extraData,
    },
  });

  return { error, paymentIntent };
}

/**
 * Confirms Alipay / Bank Redirect payment (for VNPay / MoMo flows if using Stripe).
 * Kept as a placeholder for future redirect-method support.
 */
export async function confirmRedirectPayment(stripe, clientSecret, redirectData) {
  const { error, paymentIntent } = await stripe.confirmPayment({
    clientSecret,
    confirmParams: redirectData,
    redirect: 'if_required',
  });
  return { error, paymentIntent };
}

/**
 * Destroys a Stripe card element and unmounts it from the DOM.
 */
export function destroyCardElement(cardElement) {
  if (cardElement && typeof cardElement.destroy === 'function') {
    cardElement.destroy();
  }
}

/**
 * Creates a Stripe Appearance object for the Elements iframe.
 * Extend this to match the app's design system.
 */
export const defaultStripeAppearance = {
  theme: 'stripe',
  variables: {
    colorPrimary: '#e8604c',
    colorBackground: '#ffffff',
    colorText: '#32325d',
    colorDanger: '#fa755a',
    fontFamily: '"Inter", "Helvetica Neue", Helvetica, sans-serif',
    borderRadius: '8px',
    spacingUnit: '4px',
  },
  rules: {
    '.Input': {
      border: '1px solid #e0e0e0',
      boxShadow: 'none',
      padding: '12px',
    },
    '.Input:focus': {
      border: '1px solid #e8604c',
      boxShadow: '0 0 0 2px rgba(232, 96, 76, 0.15)',
    },
    '.Label': {
      fontWeight: '500',
      marginBottom: '6px',
    },
  },
};
