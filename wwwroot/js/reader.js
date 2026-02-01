const gotoNextPage = () => {
    const currentPage = parseInt(new URLSearchParams(window.location.search).get('page')) || 1;
    const nextPage = currentPage + 1;
    window.location.search = `?page=${nextPage}`;
}

const gotoPrevPage = () => {
    const currentPage = parseInt(new URLSearchParams(window.location.search).get('page')) || 1;
    if (currentPage <= 1) return;
    const prevPage = currentPage - 1;
    window.location.search = `?page=${prevPage}`;
}

const gotoPage = (pageNumber) => {
    window.location.search = `?page=${pageNumber}`;
}

const setupReaderNavigation = () => {
  // TOC toggle
  const openToc = document.getElementById('open-toc');
  const closeToc = document.getElementById('close-toc');
  const tocOverlay = document.getElementById('toc-overlay');
  const tocSidebar = document.getElementById('toc-sidebar');

  openToc?.addEventListener('click', () => {
    tocSidebar.classList.add('active');
    tocOverlay.classList.add('active');
  });

  const closeToC = () => {
    tocSidebar.classList.remove('active');
    tocOverlay.classList.remove('active');
  };

  closeToc?.addEventListener('click', closeToC);
  tocOverlay?.addEventListener('click', closeToC);

//   // Chapter navigation
//   document.getElementById('prev-chapter-top')?.addEventListener('click', () => navigateChapter(-1));
//   document.getElementById('next-chapter-top')?.addEventListener('click', () => navigateChapter(1));
//   document.getElementById('prev-chapter-bottom')?.addEventListener('click', () => navigateChapter(-1));
//   document.getElementById('next-chapter-bottom')?.addEventListener('click', () => navigateChapter(1));

  // Keyboard navigation
  document.addEventListener('keydown', (e) => {
    if (e.key === 'ArrowLeft') gotoPrevPage();
    if (e.key === 'ArrowRight') gotoNextPage();
    if (e.key === 'Escape') closeToC();
  });

  
}

document.addEventListener('DOMContentLoaded', () => {
    const nextButtonBottom = document.getElementById('next-chapter-bottom');
    const nextButtonTop = document.getElementById('next-chapter-top');

    const prevButtonBottom = document.getElementById('prev-chapter-bottom');
    const prevButtonTop = document.getElementById('prev-chapter-top');

    nextButtonBottom?.addEventListener('click', gotoNextPage);
    nextButtonTop?.addEventListener('click',gotoNextPage);

    prevButtonBottom?.addEventListener('click', gotoPrevPage);
    prevButtonTop?.addEventListener('click', gotoPrevPage);

    setupReaderNavigation();
});