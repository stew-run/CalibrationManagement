import { useEffect, useId, useRef, useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { dropdownVariants } from '../../utils/motion';

/** Accessible menu button. `items`: [{ label, onClick, icon?, danger? }] */
export default function Dropdown({ trigger, items, align = 'right', label = 'Menu' }) {
  const [open, setOpen] = useState(false);
  const rootRef = useRef(null);
  const menuId = useId();

  useEffect(() => {
    if (!open) return undefined;
    const onPointer = (event) => { if (!rootRef.current?.contains(event.target)) setOpen(false); };
    const onKey = (event) => {
      if (event.key === 'Escape') { setOpen(false); rootRef.current?.querySelector('button')?.focus(); }
      if (event.key === 'ArrowDown' || event.key === 'ArrowUp') {
        event.preventDefault();
        const nodes = Array.from(rootRef.current?.querySelectorAll('[role="menuitem"]') ?? []);
        if (nodes.length === 0) return;
        const index = nodes.indexOf(document.activeElement);
        const next = event.key === 'ArrowDown' ? (index + 1) % nodes.length : (index - 1 + nodes.length) % nodes.length;
        nodes[next].focus();
      }
    };
    document.addEventListener('mousedown', onPointer);
    document.addEventListener('keydown', onKey);
    return () => { document.removeEventListener('mousedown', onPointer); document.removeEventListener('keydown', onKey); };
  }, [open]);

  useEffect(() => {
    if (open) rootRef.current?.querySelector('[role="menuitem"]')?.focus();
  }, [open]);

  return (
    <div className="dropdown" ref={rootRef}>
      <button type="button" className="dropdown-trigger" aria-haspopup="menu" aria-expanded={open} aria-controls={open ? menuId : undefined} aria-label={label} onClick={() => setOpen((o) => !o)}>
        {trigger}
      </button>
      <AnimatePresence>
        {open && (
          <motion.ul
            id={menuId}
            role="menu"
            className={`dropdown-menu dropdown-${align}`}
            variants={dropdownVariants}
            initial="initial"
            animate="animate"
            exit="exit"
          >
            {items.map((item) => (
              <li key={item.label} role="none">
                <button
                  type="button"
                  role="menuitem"
                  className={`dropdown-item ${item.danger ? 'danger' : ''}`}
                  onClick={() => { setOpen(false); item.onClick(); }}
                >
                  {item.label}
                </button>
              </li>
            ))}
          </motion.ul>
        )}
      </AnimatePresence>
    </div>
  );
}
