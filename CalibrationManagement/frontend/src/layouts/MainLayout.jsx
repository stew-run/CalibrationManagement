import { useEffect, useState } from 'react';
import { AnimatePresence, motion } from 'framer-motion';
import { useLocation, useOutlet } from 'react-router-dom';
import { pageVariants } from '../utils/motion';
import { useMediaQuery } from '../hooks/useMediaQuery';
import Sidebar from './Sidebar';
import Topbar from './Topbar';

export default function MainLayout() {
  const location = useLocation();
  const outlet = useOutlet();
  const mobile = useMediaQuery('(max-width: 900px)');
  const [drawerOpen, setDrawerOpen] = useState(false);
  const [collapsed, setCollapsed] = useState(() => {
    try { return localStorage.getItem('cms.sidebar') === 'collapsed'; } catch { return false; }
  });

  useEffect(() => { setDrawerOpen(false); }, [location.pathname]);

  const toggleCollapse = () => {
    setCollapsed((c) => {
      const next = !c;
      try { localStorage.setItem('cms.sidebar', next ? 'collapsed' : 'expanded'); } catch { /* storage unavailable */ }
      return next;
    });
  };

  return (
    <div className="app-shell">
      <a href="#main" className="skip-link">Skip to content</a>
      <Sidebar collapsed={collapsed} mobile={mobile} open={drawerOpen} onClose={() => setDrawerOpen(false)} onToggleCollapse={toggleCollapse} />
      <div className="app-main">
        <Topbar showMenu={mobile} onMenu={() => setDrawerOpen(true)} />
        <main id="main" className="content" tabIndex={-1}>
          <AnimatePresence mode="wait" initial={false}>
            <motion.div key={location.pathname} variants={pageVariants} initial="initial" animate="animate" exit="exit">
              {outlet}
            </motion.div>
          </AnimatePresence>
        </main>
      </div>
    </div>
  );
}
