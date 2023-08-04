$(document).ready(
    LoadDataTable()
);


var dataTable;
function LoadDataTable() {
    dataTable = $('#CompanyTable').DataTable({
        "ajax": { url: '/Admin/Company/GetAllCompany' },
        columns: [
            { data: 'name', "width": "20%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'streetAddress', "width": "15%" },
            { data: 'postalCode', "width": "10%" },
            { data: 'city', "width": "10%" },
            { data: 'state', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return ` <div class="w-75 btn-group">
                    <a href="/admin/company/upsert?id=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i> Edit</a>
                     <a onclick="deleteCompany('/admin/Company/delete/${data}')" class="btn btn-danger mx-2"><i class="bi bi-trash3"></i> Delete</a>
                </div>`
                },
                "width": "25%"
            }
        ]
    });
}

function deleteCompany(url) {
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