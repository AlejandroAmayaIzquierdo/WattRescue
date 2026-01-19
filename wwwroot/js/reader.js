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

document.addEventListener('DOMContentLoaded', () => {
    const nextButtonBottom = document.getElementById('next-chapter-bottom');
    const nextButtonTop = document.getElementById('next-chapter-top');

    const prevButtonBottom = document.getElementById('prev-chapter-bottom');
    const prevButtonTop = document.getElementById('prev-chapter-top');

    nextButtonBottom?.addEventListener('click', gotoNextPage);
    nextButtonTop?.addEventListener('click',gotoNextPage);

    prevButtonBottom?.addEventListener('click', gotoPrevPage);
    prevButtonTop?.addEventListener('click', gotoPrevPage);
});