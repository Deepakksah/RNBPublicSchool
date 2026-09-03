// Attendance helper scripts: Mark All buttons and AJAX submission
function markAllStudents(statusValue) {
    // statusValue: 1=Present, 2=Absent, 3=Leave, 4=Late
    const radioButtons = document.querySelectorAll(`input[type="radio"][value="${statusValue}"]`);
    radioButtons.forEach(rb => {
        rb.checked = true;
    });

    updateAttendanceCounters();
}

function updateAttendanceCounters() {
    let present = 0, absent = 0, leave = 0, late = 0;
    const allChecked = document.querySelectorAll('input[type="radio"]:checked');

    allChecked.forEach(rb => {
        if (rb.value === '1' || rb.value === 'Present') present++;
        else if (rb.value === '2' || rb.value === 'Absent') absent++;
        else if (rb.value === '3' || rb.value === 'Leave') leave++;
        else if (rb.value === '4' || rb.value === 'Late') late++;
    });

    const pEl = document.getElementById('countPresent');
    const aEl = document.getElementById('countAbsent');
    const lEl = document.getElementById('countLeave');
    const ltEl = document.getElementById('countLate');

    if (pEl) pEl.innerText = present;
    if (aEl) aEl.innerText = absent;
    if (lEl) lEl.innerText = leave;
    if (ltEl) ltEl.innerText = late;
}

document.addEventListener('DOMContentLoaded', function () {
    // Dynamic Class to Section cascading dropdown
    const classSelect = document.getElementById('filterClassSelect');
    const sectionSelect = document.getElementById('filterSectionSelect');

    if (classSelect && sectionSelect) {
        classSelect.addEventListener('change', function () {
            const classId = this.value;
            sectionSelect.innerHTML = '<option value="">-- Loading Sections... --</option>';

            if (!classId) {
                sectionSelect.innerHTML = '<option value="">-- All Sections --</option>';
                return;
            }

            fetch(`/Student/GetSectionsByClass?classId=${classId}`)
                .then(res => res.json())
                .then(data => {
                    sectionSelect.innerHTML = '<option value="">-- Select Section --</option>';
                    data.forEach(sec => {
                        const opt = document.createElement('option');
                        opt.value = sec.id;
                        opt.textContent = sec.name;
                        sectionSelect.appendChild(opt);
                    });
                })
                .catch(err => {
                    console.error('Error fetching sections:', err);
                    sectionSelect.innerHTML = '<option value="">-- Failed to load --</option>';
                });
        });
    }

    // Attach counter listeners on radio changes
    document.querySelectorAll('.attendance-radio').forEach(rb => {
        rb.addEventListener('change', updateAttendanceCounters);
    });

    updateAttendanceCounters();
});
