$(document).ready(() => {
    var url = window.location.search;
    if (url.includes("pending")) {
    LoadDataTable("pending");
    }
    else if(url.includes("inprocess")) {
    LoadDataTable("inprocess");
    }
    else if(url.includes("completed")) {
    LoadDataTable("completed");
    }
    else if(url.includes("approved")) {
    LoadDataTable("approved");
    }
    else{
        LoadDataTable("all");
    }
});


var dataTable;
function LoadDataTable(status) {
    dataTable = $('#OrderTable').DataTable({
        "ajax": { url: `/Admin/Order/GetAll?status=${status}` },
        columns: [
            { data: 'id', "width": "5%" },
            { data: 'name', "width": "15%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'applicationUser.email', "width": "20%" },
            { data: 'orderStatus', "width": "10%" },
            { data: 'orderTotal', "width": "10%" },
            {
                data: 'id',
                "render": function (data) {
                    return ` <div class="w-75 btn-group">
                    <a href="/admin/order/details?orderId=${data}" class="btn btn-primary mx-2"><i class="bi bi-pencil-square"></i></a>
                </div>`
                },
                "width": "5%"
            }
        ]
    });
}

