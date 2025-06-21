// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', function () {
    const searchInput = document.getElementById('search-input');
    const suggestionsContainer = document.getElementById('search-suggestions-container');
    let debounceTimer;

    if (searchInput) {
        searchInput.addEventListener('input', function () {
            const term = this.value;

            // Xóa timer cũ nếu người dùng gõ tiếp
            clearTimeout(debounceTimer);

            if (term.length < 2) {
                suggestionsContainer.innerHTML = '';
                suggestionsContainer.style.display = 'none';
                return;
            }

            // Đặt timer mới, chỉ gửi request sau khi người dùng ngưng gõ 300ms
            debounceTimer = setTimeout(() => {
                // Gọi API để lấy gợi ý
                fetch(`/api/search-suggestions?term=${encodeURIComponent(term)}`)
                    .then(response => {
                        if (!response.ok) {
                            throw new Error('Network response was not ok');
                        }
                        return response.json();
                    })
                    .then(data => {
                        suggestionsContainer.innerHTML = ''; // Xóa gợi ý cũ
                        if (data.length > 0) {
                            suggestionsContainer.style.display = 'block';
                            data.forEach(item => {
                                // Tạo link cho mỗi gợi ý
                                const link = document.createElement('a');
                                link.href = `/san-pham/${item.slug}`;

                                // Tạo ảnh
                                const img = document.createElement('img');
                                // Dùng ảnh mặc định nếu sản phẩm không có ảnh
                                img.src = item.imageUrl ? item.imageUrl : '/images/default-product.png';
                                img.alt = item.name;

                                // Tạo tên sản phẩm
                                const text = document.createTextNode(item.name);

                                link.appendChild(img);
                                link.appendChild(text);
                                suggestionsContainer.appendChild(link);
                            });
                        } else {
                            suggestionsContainer.style.display = 'none';
                        }
                    })
                    .catch(error => {
                        console.error('Lỗi khi lấy gợi ý tìm kiếm:', error);
                        suggestionsContainer.style.display = 'none';
                    });
            }, 300); // 300ms delay
        });

        // Ẩn gợi ý khi click ra ngoài ô tìm kiếm
        document.addEventListener('click', function (e) {
            if (!e.target.closest('#search-form')) {
                suggestionsContainer.style.display = 'none';
            }
        });
    }
});