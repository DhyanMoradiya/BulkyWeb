$(document).ready(
    LoadDataTable()
);


var dataTable;
function LoadDataTable() {
    dataTable = $('#UserTable').DataTable({
        "ajax": { url: '/Admin/User/GetAll' },
        columns: [
            { data: 'name', "width": "15%" },
            { data: 'email', "width": "20%" },
            { data: 'phoneNumber', "width": "15%" },
            { data: 'company.name', "width": "15%" },
            { data: 'role', "width": "10%" },
            {
                data: { id: 'id', lockoutEnd: 'lockoutEnd' },
                "render": function (data) {
                    var today = new Date().getTime();
                    var lockout = new Date(data.lockoutEnd).getTime();
                    if (today < lockout) {

                        return ` <div class="text-center">
                    <a  onclick="lockunlock('${data.id}')" class="btn btn-success text-white mx-2" style="cursor:pointer; width:100px">
                        <i class="bi bi-unlock-fill"></i> Unlock
                    </a>
                    
                     <a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-success text-white mx-2" style="cursor:pointer; width:120px">
                        <i class="bi bi-pencil-square"></i> Permission
                    </a>
                        </div>`
                    } else {
                        return ` <div class="text-center">
                    <a onclick="lockunlock('${data.id}')" class="btn btn-danger text-white mx-2" style="cursor:pointer; width:100px">
                        <i class="bi bi-lock-fill"></i> lock
                    </a>
                    
                     <a href="/admin/user/RoleManagement?userId=${data.id}" class="btn btn-success text-white mx-2" style="cursor:pointer; width:150px">
                        <i class="bi bi-pencil-square"></i> Permission
                    </a>
                     </div>`
                    }
                },
                "width": "25%"

            }
        ]
    });
}

function lockunlock(id) {
    $.ajax({
        url: "/Admin/User/LockUnlock",
        type: "POST",
        data: JSON.stringify(id),
        contentType: "application/json",
        success: (data) => {
            if (data.success) {
                toastr.success(data.message);
                dataTable.ajax.reload();
            } else {
                toastr.error(success.message);
            }

        }
    })
}