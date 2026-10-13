import { useEffect, useId, useRef, useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { dropdownVariants } from '../../utils/motion';
import { equipmentApi } from '../../api/index';
import { useDebounce } from '../../hooks/useDebounce';
import Spinner from './Spinner';

/** Searchable equipment selector backed by GET /api/equipment. */
export default function EquipmentPicker({ id, value, label, onChange, disabled, invalid, describedBy }) {
  const listId = useId();
  const rootRef = useRef(null);
  const [text, setText] = useState(label || '');
  const [open, setOpen] = useState(false);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [items, setItems] = useState([]);
  const debounced = useDebounce(text, 300);

  // Mirror the selected label; never wipe what the user is typing when the selection is cleared.
  useEffect(() => { if (label) setText(label); }, [label]);

  useEffect(() => {
    if (!open) return undefined;
    const controller = new AbortController();
    setLoading(true);
    setError(null);
    equipmentApi.search({ search: debounced === label ? '' : debounced, pageSize: 12, sortBy: 'code' }, controller.signal)
      .then((page) => { if (!controller.signal.aborted) { setItems(page.items); setLoading(false); } })
      .catch((e) => { if (!controller.signal.aborted && e?.name !== 'CanceledError') { setError(e); setLoading(false); } });
    return () => controller.abort();
  }, [debounced, open, label]);

  useEffect(() => {
    if (!open) return undefined;
    const onPointer = (event) => { if (!rootRef.current?.contains(event.target)) setOpen(false); };
    document.addEventListener('mousedown', onPointer);
    return () => document.removeEventListener('mousedown', onPointer);
  }, [open]);

  const select = (item) => {
    const itemLabel = `${item.equipmentCode} – ${item.equipmentName}`;
    setText(itemLabel);
    setOpen(false);
    onChange(item.equipmentId, itemLabel, item);
  };

  return (
    <div className="picker" ref={rootRef}>
      <input
        id={id}
        className="input"
        type="text"
        role="combobox"
        aria-expanded={open}
        aria-controls={open ? listId : undefined}
        aria-autocomplete="list"
        aria-invalid={invalid || undefined}
        aria-describedby={describedBy}
        autoComplete="off"
        placeholder="Search by code or name…"
        value={text}
        disabled={disabled}
        onFocus={() => setOpen(true)}
        onChange={(e) => { setText(e.target.value); setOpen(true); if (value) onChange('', '', null); }}
        onKeyDown={(e) => { if (e.key === 'Escape') setOpen(false); }}
      />
      <AnimatePresence>
        {open && (
          <motion.ul id={listId} role="listbox" className="picker-list" variants={dropdownVariants} initial="initial" animate="animate" exit="exit">
            {loading ? <li className="picker-note"><Spinner size={14} /> Searching…</li> : null}
            {error ? <li className="picker-note form-error">{error.message}</li> : null}
            {!loading && !error && items.length === 0 ? <li className="picker-note">No matching equipment</li> : null}
            {items.map((item) => (
              <li key={item.equipmentId} role="option" aria-selected={item.equipmentId === value}>
                <button type="button" className="picker-option" onClick={() => select(item)}>
                  <strong>{item.equipmentCode}</strong>
                  <span>{item.equipmentName}</span>
                  <small className="muted">{item.departmentName || ''}</small>
                </button>
              </li>
            ))}
          </motion.ul>
        )}
      </AnimatePresence>
    </div>
  );
}
