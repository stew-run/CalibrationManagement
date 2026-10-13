// Shared animation vocabulary: one set of springs and variants used across the whole app.
export const spring = {
  gentle: { type: 'spring', stiffness: 260, damping: 28, mass: 0.9 },
  snappy: { type: 'spring', stiffness: 420, damping: 32 },
  soft: { type: 'spring', stiffness: 180, damping: 24 }
};

export const pageVariants = {
  initial: { opacity: 0, y: 10 },
  animate: { opacity: 1, y: 0, transition: { ...spring.gentle } },
  exit: { opacity: 0, y: -6, transition: { duration: 0.12 } }
};

export const fadeVariants = {
  initial: { opacity: 0 },
  animate: { opacity: 1, transition: { duration: 0.18 } },
  exit: { opacity: 0, transition: { duration: 0.12 } }
};

export const staggerContainer = {
  initial: {},
  animate: { transition: { staggerChildren: 0.06, delayChildren: 0.04 } }
};

export const cardVariants = {
  initial: { opacity: 0, y: 14, scale: 0.98 },
  animate: { opacity: 1, y: 0, scale: 1, transition: { ...spring.gentle } }
};

export const modalVariants = {
  initial: { opacity: 0, y: 24, scale: 0.97 },
  animate: { opacity: 1, y: 0, scale: 1, transition: { ...spring.snappy } },
  exit: { opacity: 0, y: 12, scale: 0.98, transition: { duration: 0.12 } }
};

export const overlayVariants = fadeVariants;

export const dropdownVariants = {
  initial: { opacity: 0, y: -6, scale: 0.97 },
  animate: { opacity: 1, y: 0, scale: 1, transition: { ...spring.snappy } },
  exit: { opacity: 0, y: -4, scale: 0.98, transition: { duration: 0.1 } }
};

export const toastVariants = {
  initial: { opacity: 0, x: 40, scale: 0.96 },
  animate: { opacity: 1, x: 0, scale: 1, transition: { ...spring.snappy } },
  exit: { opacity: 0, x: 40, transition: { duration: 0.15 } }
};

export const rowVariants = {
  initial: { opacity: 0 },
  animate: { opacity: 1, transition: { duration: 0.18 } },
  exit: { opacity: 0, transition: { duration: 0.1 } }
};

export const listItemVariants = {
  initial: { opacity: 0, x: -8 },
  animate: { opacity: 1, x: 0, transition: { ...spring.gentle } },
  exit: { opacity: 0, x: 8, transition: { duration: 0.1 } }
};

export const buttonMotion = {
  whileHover: { y: -1 },
  whileTap: { scale: 0.97 },
  transition: spring.snappy
};

export const shakeVariants = {
  initial: { x: 0 },
  shake: { x: [0, -8, 8, -5, 5, 0], transition: { duration: 0.35 } }
};
