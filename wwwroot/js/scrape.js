const ShowAlert = (type, message) => {
    const alertBox = document.getElementById(`alert-${type}`);

    alertBox.querySelector('span').textContent = message;

    alertBox.classList.remove('hidden');
}

const HideAlert = (type) => {
    const alertBox = document.getElementById(`alert-${type}`);
    if (!alertBox) return;

    if (alertBox.classList.contains('hidden')) return;

    alertBox.classList.add('hidden');
}

const ShowBookPreview = (story) => {
    const previewContainer = document.getElementById('book-preview');

    if (!previewContainer.classList.contains('hidden')) return;
    previewContainer.classList.remove('hidden');

    const previewTitle = document.getElementById('preview-title');
    previewTitle.textContent = story.title;

    const previewCover = document.getElementById('preview-cover');

    if (story.cover) {
        previewCover.src = story.cover;
        previewCover.style.display = 'block';
    }
}

const HideBookPreview = () => {
    const previewContainer = document.getElementById('book-preview');
    if (previewContainer.classList.contains('hidden')) return;
    previewContainer.classList.add('hidden');
}

const HideProgress = () => {
    const progressContainer = document.getElementById('progress-container');

    if (progressContainer.classList.contains('hidden')) return;
    progressContainer.classList.add('hidden');
}

const UpdateProgress = (message, percentage) => {
    const progressContainer = document.getElementById('progress-container');

    if (progressContainer.classList.contains('hidden'))
        progressContainer.classList.remove('hidden');

    const progressStatus = document.getElementById('progress-status');

    if (message) {
        progressStatus.textContent = message;
    }
    const progressBar = document.getElementById('progress-fill');

    progressBar.style.width = `${percentage}%`;


}


document.addEventListener('DOMContentLoaded', () => {
    const form = document.getElementById('scrape-form');

    form.addEventListener('submit', async (e) => {
        e.preventDefault();

        console.log('Form submitted');

        const data = new FormData(e.target);

        const url = data.get('url');
        
        console.log('URL to scrape:', url);


        // const re = new RegExp('^https?:\/\/(www\.)?wattpad\.com\/story\/\d+(?:-[A-Za-z0-9%]+)*$');

        // if (!re.test(url)) {
        //     alert('Please enter a valid Wattpad story URL.');
        //     return;
        // }
        
        try {
            const response = await fetch('/api/scrape/start', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: JSON.stringify({ StoryUrl: url }),
            });
            const result = await response.json();
            console.log('Scrape started:', result);

            HideAlert('error');
            HideAlert('success');

            ShowBookPreview(result);

            const intervalId = setInterval(async () => {
                try {
                    const progressResponse = await fetch(`/api/scrape/progress/${result.id}`);
                    const progressData = await progressResponse.json();
                    console.log('Scrape progress:', progressData);

                    if (progressData.isError == true) {
                        HideAlert('success');
                        ShowAlert('error', progressData.message);
                        HideProgress();
                        clearInterval(intervalId);
                        return;
                    }

                    if (progressData.isCompleted == true || progressData.percentage >= 100) {
                        HideAlert('error');
                        ShowAlert('success', 'Scrape complete!');
                        UpdateProgress('Scrape complete!', 100);
                        clearInterval(intervalId);
                        return;
                    }

                    UpdateProgress(progressData.message, progressData.percentage);
                } catch (error) {
                    console.error('Error fetching progress:', error);
                    clearInterval(intervalId);
                }
            }, 300);
            
        } catch (error) {
            ShowAlert('error', 'Error starting scrape');
            console.error('Error starting scrape:', error);
        }
    });
});