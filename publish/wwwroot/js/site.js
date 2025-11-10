

function openPopup(id) {
    document.querySelectorAll('.popup-overlay').forEach(popup => {
        popup.style.display = 'none';
    });

    const popup = document.getElementById(id);
    if (popup) {
        popup.style.display = 'block';
        document.body.style.overflow = 'hidden';
    }
}

function closePopup(id) {
    const popup = document.getElementById(id);
    if (popup) {
        popup.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
}

document.addEventListener('click', function (event) {
    if (event.target.classList.contains('popup-overlay')) {
        event.target.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
});

document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape') {
        document.querySelectorAll('.popup-overlay').forEach(popup => {
            popup.style.display = 'none';
        });
        document.body.style.overflow = 'auto';
    }
});

async function handleSignup(event) {
    event.preventDefault();
    console.log(event);
    
    const form = document.getElementById('signup-form');
    const errorDiv = document.getElementById('signup-error');
    const formData = new FormData(form);

     const data = {
         Email: formData.get('Email'),
         Password: formData.get('Password'),
         ConfirmPassword: formData.get('ConfirmPassword'),
         Role: formData.get('Role')
     };
     console.log(data);
    // const data={
    //     Email:"a@gmail.com",
    //     Password:"123",
    //     ConfirmPassword:"123",
    //     Role:"Doctor"
    // }

    // Basic client-side validation
    if (data.Password !== data.ConfirmPassword) {
        errorDiv.style.display = 'block';
        errorDiv.textContent = 'Passwords do not match';
        return;
    }

    try {
        const response = await fetch('http://localhost:5157/api/auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            
            body: JSON.stringify(data)
        });

        const result = await response.json();
        console.log(result);
    
    

        if (response.ok) {
            errorDiv.style.display = 'none';
            
            alert(result.Message); // Show success message
            form.reset(); // Reset form
            closePopup('signup-popup'); // Close popup
        } else {
            errorDiv.style.display = 'block';
            errorDiv.textContent = result.message || 'Registration failed';
        }
    } catch (error) {
        errorDiv.style.display = 'block';
        errorDiv.textContent = 'An error occurred. Please try again.';
        console.error('Error:', error);
    }
}



function openPopup(id) {
    document.querySelectorAll('.popup-overlay').forEach(popup => {
        popup.style.display = 'none';
    });

    const popup = document.getElementById(id);
    if (popup) {
        popup.style.display = 'block';
        document.body.style.overflow = 'hidden';
    }
}

function closePopup(id) {
    const popup = document.getElementById(id);
    if (popup) {
        popup.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
}

document.addEventListener('click', function (event) {
    if (event.target.classList.contains('popup-overlay')) {
        event.target.style.display = 'none';
        document.body.style.overflow = 'auto';
    }
});

document.addEventListener('keydown', function (event) {
    if (event.key === 'Escape') {
        document.querySelectorAll('.popup-overlay').forEach(popup => {
            popup.style.display = 'none';
        });
        document.body.style.overflow = 'auto';
    }
});

async function handleSignup(event) {
    event.preventDefault();
    
    const form = document.getElementById('signup-form');
    const errorDiv = document.getElementById('signup-error');
    const formData = new FormData(form);

    const data = {
        Email: formData.get('Email'),
        Password: formData.get('Password'),
        ConfirmPassword: formData.get('ConfirmPassword'),
        Role: formData.get('Role')
    };

    // Basic client-side validation
    if (data.Password !== data.ConfirmPassword) {
        errorDiv.style.display = 'block';
        errorDiv.textContent = 'Passwords do not match';
        return;
    }

    try {
        const response = await fetch('http://localhost:5157/api/auth/register', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (response.ok) {
            errorDiv.style.display = 'none';
            form.reset();
            closePopup('signup-popup');
            // Redirect to /verify with email as query parameter
            window.location.href = `/verify?email=${encodeURIComponent(data.Email)}`;
        } else {
            errorDiv.style.display = 'block';
            errorDiv.textContent = result.message || 'Registration failed';
        }
    } catch (error) {
        errorDiv.style.display = 'block';
        errorDiv.textContent = 'An error occurred. Please try again.';
        console.error('Error:', error);
    }
}

async function handleVerify(event) {
    event.preventDefault();
    
    const form = document.getElementById('verify-form');
    const errorDiv = document.getElementById('verify-error');
    const formData = new FormData(form);

    const data = {
        Email: formData.get('Email'),
        Code: formData.get('Code')
    };

    try {
        const response = await fetch('http://localhost:5157/api/auth/verify-email', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(data)
        });

        const result = await response.json();

        if (response.ok) {
            errorDiv.style.display = 'none';
            alert(result.Message); // Show success message
            form.reset();
            // Redirect to /home
            window.location.href = '/home';
        } else {
            errorDiv.style.display = 'block';
            errorDiv.textContent = result.message || 'Verification failed';
        }
    } catch (error) {
        errorDiv.style.display = 'block';
        errorDiv.textContent = 'An error occurred. Please try again.';
        console.error('Error:', error);
    }
}

async function handleLogin(event) {
    event.preventDefault();

    const email = document.querySelector('#login-popup input[name="Email"]').value.trim();
    const password = document.querySelector('#login-popup input[name="Password"]').value.trim();
    const payload = { Email: email, Password: password };

    try {
        const response = await fetch('http://localhost:5157/api/auth/login', {
            method: 'POST',
            headers: { 'Content-Type': 'application/json' },
            credentials: 'include', // important for cookie
            body: JSON.stringify(payload)
        });

        const result = await response.json();

        if (!response.ok) {
            alert("Login failed: " + (result.message || "Something went wrong"));
            return;
        }

        const role = result.role;

        if (role === "Passenger") {
            window.location.href = "/Passenger";
        } else if (role === "Driver") {
            window.location.href = "/Driver";
        } else if (role === "Admin") {
            window.location.href = "/Admin";
        } else {
            alert("Unrecognized role.");
        }

    } catch (error) {
        console.error("Login error:", error);
        alert("An error occurred during login.");
    }
}

