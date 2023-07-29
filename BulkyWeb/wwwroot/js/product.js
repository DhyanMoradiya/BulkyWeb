$(document).ready(
    loadTable()
);

var dataTable;
function loadTable() {
    dataTable = $('#productTable').DataTable({
        "ajax": { url: '/Admin/Product/getallproducts' },
        columns: [
            { data: 'title', "width": "20%" },
            { data: 'author', "width": "15%" },
            { data: 'isbn', "width": "15%" },
            { data: 'price', "width": "10%" },
            { data: 'category.name', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return ` <div class="w-75 btn-group">
                    <a href="/admin/product/upsert?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                     <a onclick="deleteProduct('/admin/product/delete/${data}')" class="btn btn-danger mx-2"><i class="bi bi-trash3"></i> Delete</a>
                </div>`
                },
                "width": "25%"
            }
        ]
    });
}

function deleteProduct(url) {
    console.log("called");
    Swal.fire({
        title: 'Are you sure?',
        text: "You won't be able to revert this!",
        icon: 'warning',
        showCancelButton: true,
        confirmButtonColor: '#3085d6',
        cancelButtonColor: '#d33',
        confirmButtonText: 'Yes, delete it!'
    }).then((result) => {
        if (result.isConfirmed) {
            Swal.fire(
                $.ajax({
                    url: url,
                    type: "delete",
                    success: (data) => {
                        Swal.fire(data.message);
                        dataTable.ajax.reload()
                    }
                })
            )
        }
    })
}