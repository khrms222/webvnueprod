$(document).ready(function () {

    $("#email-button").click(function (event) {
        $.ajax({
            type: "POST",
            url: "/account/ConfirmEmail",
            error: function (XMLHttpRequest, textStatus, errorThrown) {
                $("#email-button").replaceWith("Error, try again later")
            },
            success: function (result) {
                $("#email-button").replaceWith("Sent!")
            }

        });
        return false;
    });

    $("#refbutton").click(function (event) {
        $("#referral-link").show();
    });

    $('#Country').change(function () {
        
        var countryName = $('#Country').find(":selected").text();
        var usStatesHtml = '<select class="form-control" id="State" name="state"><option value="AL">Alabama</option><option value="AK">Alaska</option><option value="AZ">Arizona</option><option value="AR">Arkansas</option><option value="CA">California</option><option value="CO">Colorado</option><option value="CT">Connecticut</option><option value="DE">Delaware</option><option value="DC">District Of Columbia</option><option value="FL">Florida</option><option value="GA">Georgia</option><option value="HI">Hawaii</option><option value="ID">Idaho</option><option value="IL">Illinois</option><option value="IN">Indiana</option><option value="IA">Iowa</option><option value="KS">Kansas</option><option value="KY">Kentucky</option><option value="LA">Louisiana</option><option value="ME">Maine</option><option value="MD">Maryland</option><option value="MA">Massachusetts</option><option value="MI">Michigan</option><option value="MN">Minnesota</option><option value="MS">Mississippi</option><option value="MO">Missouri</option><option value="MT">Montana</option><option value="NE">Nebraska</option><option value="NV">Nevada</option><option value="NH">New Hampshire</option><option value="NJ">New Jersey</option><option value="NM">New Mexico</option><option value="NY">New York</option><option value="NC">North Carolina</option><option value="ND">North Dakota</option><option value="OH">Ohio</option><option value="OK">Oklahoma</option><option value="OR">Oregon</option><option value="PA">Pennsylvania</option><option value="RI">Rhode Island</option><option value="SC">South Carolina</option><option value="SD">South Dakota</option><option value="TN">Tennessee</option><option value="TX">Texas</option><option value="UT">Utah</option><option value="VT">Vermont</option><option value="VA">Virginia</option><option value="WA">Washington</option><option value="WV">West Virginia</option><option value="WI">Wisconsin</option><option value="WY">Wyoming</option></select>'
        var notUsHtml = '<input class="form-control" id="State" name="State" placeholder="State" type="text" value="" />'
        if (countryName == "United States") {
            $('#State').replaceWith(usStatesHtml);
        }
        if (countryName != "United States"){
            $('#State').replaceWith(notUsHtml);
        }

    });

    $("#unsubscribe-button").click(function(event){  
        var input = confirm("Do you wish to unsubscribe from unsubscribing from the unsubscribe?")

        if (input == true) {
            $.ajax({
                type: "POST",
                url: "/services/unsubscribe",
                error: function (XMLHttpRequest, textStatus, errorThrown) {
                    alert("There was an error. Please try again later")
                },
                success: function (result) {
                    $('#accountinfo').load('/account/settings #accountinfo')
                }

            });
        }
        else {
            alert("There was an error. Please try again later")
        }
    });

    $("#upload-button").click(function (event) {
        $("#uploadphoto").show();
        $("#uploadphotobtn").hide();
    });
    $("#upload-mainbtn").click(function (event) {
        $("#uploadmainphoto").show();
        $("#upload-mainbtn").hide();
    });

    $("#editbio").click(function (event) {
        $("#save-user-bio").show();
        $("#editbio").hide();
    });

    $("#editbio").click(function (event) {
        var id = $("#editbio").attr("user-id")

        var datavalue = { "id" : id};

        $.ajax({
            url: '/Home/ajaxUserProfileBio',
            type: 'POST',
            data: datavalue,
            dataType: 'json',
            success: function (data) {
                $('#bio-aboutme').replaceWith('<textarea rows="5" cols="30"  id = "bioAboutMe"> ' + data['Bio'].AboutMe + ' </textarea>')
                $('#bio-location').replaceWith('<textarea rows="1" cols="30"  id = "bioLocation"> ' + data['Bio'].Location + ' </textarea>');
                $('#bio-gender').replaceWith('<textarea rows="1" cols="30"  id = "bioGender"> ' + data['Bio'].Gender + ' </textarea>');
                $('#bio-quote').replaceWith('<textarea rows="5" cols="30"  id = "bioQuote"> ' + data['Bio'].Quote + ' </textarea>');
            },
            error: function (request, error) {
                alert("Request: " + JSON.stringify(request));
            }
        });
    });

    $("#savebio").click(function (event) {
        var id = $("#savebio").attr("user-id")

        var aboutme = $("#bioAboutMe").val();
        var location = $("#bioLocation").val();
        var gender = $("#bioGender").val();
        var quote = $("#bioQuote").val();

        var datavalue = { "id": id,"AboutMe": aboutme, "Location": location, "Gender": gender, "Quote": quote};

        $.ajax({
            url: '/Home/saveBio',
            type: 'POST',
            data: datavalue,
            success: function (data) {
                window.location.reload();
            },
            error: function (request, error) {
                alert("Request: " + JSON.stringify(request));
            }
        });
    });

    $("#search-button").click(function (event) {
        var searchdata = $("#search-input").val();
        
        if (searchdata.length != 0) {
            window.location.href = "/search/users?query=" + searchdata;
        }
    });

    $('#search-input').keypress(function (e) {
        if (e.keyCode == 13)
            $('#search-button').click();
    });

    $("#follow-button").click(function (event) {
        var id = $("#follow-button").attr("user-id")

        var datavalue = { "id": id };

        $.ajax({
            url: '/Home/addFollowing',
            type: 'POST',
            data: datavalue,
            success: function (data) {
                $("#unfollow-button").show();
                $("#follow-button").hide();
            },
            error: function (request, error) {
                alert("Request: " + JSON.stringify(request));
            }
        });
    });

    $("#unfollow-button").click(function (event) {
        var id = $("#unfollow-button").attr("user-id")

        var datavalue = { "id": id };

        $.ajax({
            url: '/Home/removeFollowing',
            type: 'POST',
            data: datavalue,
            success: function (data) {
                $("#follow-button").show();
                $("#unfollow-button").hide();
            },
            error: function (request, error) {
                alert("Request: " + JSON.stringify(request));
            }
        });
    });

    /*
    $("#add-comment").click(function (event) {
        var id = $("#add-comment").attr("user-id")
        var postid = $("#add-comment").attr("post-id")
        var message = $("#user-comment").val();

        var datavalue = { "id": id, "PostId": postid, "Message": message };

        $.ajax({
            url: '/Home/addComment',
            type: 'POST',
            data: datavalue,
            success: function (data) {
                window.location.reload();
            },
            error: function (request, error) {
                alert("Request: " + JSON.stringify(request));
            }
        });
    });
    */

    /*
    $(".commentButton").click(function (event) {
        var stringId = $(this).attr('id');
        var id = $(this.id).attr("user-id")
        var postid = $(this.id).attr("post-id")
        var message = document.getElementById(stringId).getElementsByClassName("commentText")[0];

        var datavalue = { "id": id, "PostId": postid, "Message": message };

        $.ajax({
            url: '/Home/addComment',
            type: 'POST',
            data: datavalue,
            success: function (data) {
                window.location.reload();
            },
            error: function (request, error) {
                alert("Request: " + JSON.stringify(request));
            }
        });
    });
    */

    
    $(".commentButton").click(function (event) {
        var commentButtonId = $(this).attr('id');

        var id = $(this).attr("user-id")
        var userName = $(this).attr("user-username")
        var userFullName = $(this).attr("user-fullname")
        var postid = $(this).attr('post-id')

        var elements = document.getElementsByClassName("commentText");
        var message;

        for (i = 0; i < elements.length; i++) {
            if (elements[i].id == commentButtonId) {
                message = elements[i];
            }
        }

        var datavalue = { "id": id, "PostId": postid, "Message": message.value };

        $.ajax({
            url: '/Home/addComment',
            type: 'POST',
            data: datavalue,
            success: function (data) {
                //window.location.reload();
                //$('.myBox').load(document.URL + ' .myBox');
                //$(".myBox").load(location.href + " .myBox>*", "");

                /*
                var commentDivs = document.getElementsByClassName("myBox");

                for (i = 0; i < commentDivs.length; i++) {
                    $('#' + commentDivs[i].id).load(document.URL + ' #' + commentDivs[i].id);
                    message.value = "";
                }*/
                
                $('#' + postid + '-ul').append('<li id="commentText"><img class="img-thumbnail img-responsive" src="/Home/profileimg/' + id + '" width="30" height="30" /><a href="~/' + userName + '"><b>' + userFullName + '</b></a> : ' + message.value + '<span class="pull-right"><small>6/7/2017</small></span></li>');
                message.value = "";

                var scrollBar = document.getElementById(postid + '-scrollbar');
                scrollBar.scrollTop = scrollBar.scrollHeight;
                
            },
            error: function (request, error) {
                alert("Request: " + JSON.stringify(request));
            }
        });

    });

    $(".deletepost").click(function (event) {
        var postid = $(this).attr("post-id")

        var datavalue = {"PostId": postid};

        $.ajax({
            url: '/Home/DeletePost',
            type: 'POST',
            data: datavalue,
            success: function (data) {
                window.location.reload();
            },
            error: function (request, error) {
                alert("Request: " + JSON.stringify(request));
            }
        });
    });
    

    function textChangeListener(evt) {
        var id = evt.target.id;
        var text = evt.target.value;

        if (id == "topLineText") {
            window.topLineText = text;
        } else {
            window.bottomLineText = text;
        }

        redrawMeme(window.imageSrc, window.topLineText, window.bottomLineText);
    }
    //------
    function redrawMeme(image, topLine, bottomLine) {
        // Get Canvas2DContext
        var canvas = document.querySelector('canvas');
        var ctx = canvas.getContext("2d");
        if (image != null)
            ctx.drawImage(image, 0, 0, canvas.width, canvas.height);

        // clear previous
        ctx.clearRect(0, 0, canvas.width, canvas.height);

        if (image != null)
            ctx.drawImage(image, 0, 0, canvas.width, canvas.height);

        // Text attributes
        ctx.font = '30pt Impact';
        ctx.textAlign = 'center';
        ctx.strokeStyle = 'black';
        ctx.lineWidth = 3;
        ctx.fillStyle = 'white';

        if (topLine != null) {
            ctx.fillText(topLine, canvas.width / 2, 40);
            ctx.strokeText(topLine, canvas.width / 2, 40);
        }

        if (bottomLine != null) {
            ctx.fillText(bottomLine, canvas.width / 2, canvas.height - 20);
            ctx.strokeText(bottomLine, canvas.width / 2, canvas.height - 20);
        }
    }

    function saveFile() {
        window.open(document.querySelector('canvas').toDataURL());
    }
    //--------

    function handleFileSelect(evt) {
        //make canvas
        var canvasWidth = 400;
        var canvasHeight = 400;
        var file = evt.target.files[0];

        //image upload
        var reader = new FileReader();
        reader.onload = function (fileObject) {
            var data = fileObject.target.result;

            // Create an image object
            var image = new Image();
            image.onload = function () {

                window.imageSrc = this;
                redrawMeme(window.imageSrc, null, null);
            }

            // Set image data to background image.
            image.src = data;
            console.log(fileObject.target.result);
        };
        reader.readAsDataURL(file)
    }
    window.imageSRC = null;
    window.topLineText = "";
    window.bottomLineText = "";
    window.imageSRC = null;
    window.topLineText = null;
    window.bottomLineText = null;

    var file = document.querySelector("#file");
    file.onchange = handleFileSelect;

    var input1 = document.getElementById('topLineText');
    //var input2 = document.getElementById('bottomLineText');
    input1.oninput = textChangeListener;
    //input2.oninput = textChangeListener;
    document.getElementById('file').addEventListener('change', handleFileSelect, false);
    document.querySelector('button').addEventListener('click', saveFile, false);


    /*
    window.onload = function () {
        var objDivs = document.getElementsByClassName('myBox')
        for (i = 0; i < objDivs.length; i++) {
            var target = document.getElementById(objDivs[i].id)
            //target.scrollTop = target.scrollHeight;
            console.log(target)
        }

    };
    */

    window.onload = function () {
        var objDiv = document.getElementsByClassName("scrollbar");
        for (i = 0; i < objDiv.length; i++) {
            var target = document.getElementById(objDiv[i].id)
            target.scrollTop = target.scrollHeight;
        }

    }

    function convertDateTime(date) {
        var hours = date.getHours();
        var ampm = hours >= 12 ? 'PM' : 'AM';
        if (hours > 12) {
            hours = hours - 12
        }
        if (hours == 0) {
            hours = 12
        }
        date = ('' + (date.getUTCMonth() + 1)).slice(-2) + '/' +
            ('00' + date.getDate()).slice(-2) + '/' +
            date.getUTCFullYear() + ' ' +
            hours + ':' +
            ('00' + date.getUTCMinutes()).slice(-2) + ':' +
            ('00' + date.getUTCSeconds()).slice(-2) + ' ' + ampm;
        return date

    }

    
    function yourFunction() {
        var commentDivs = document.getElementsByClassName("myBox");

        
        for (i = 0; i < commentDivs.length; i++) {
            //$('#' + commentDivs[i].id).load(document.URL + ' #' + commentDivs[i].id);

            var postid = commentDivs[i].getAttribute("post-id");
            
            var comments = $('#' + commentDivs[i].id).find('.commentTextClass');

            if (typeof comments[comments.length - 1] != 'undefined') {
                var timestamp = comments[comments.length - 1].getAttribute("comment-timestamp")

                var datavalue = { "PostId": postid, "TimeStamp": timestamp };

                $.ajax({
                    url: '/Home/CheckIfCommentsUpdated',
                    type: 'POST',
                    data: datavalue,
                    success: function (data) {
                        /*
                        var date = data['Result'];
                        date = new Date(parseInt(date.replace("/Date(", "").replace(")/", ""), 10));
                        
                        console.log('DB MAX: ' + convertDateTime(date));
                        
                        timestamp = new Date(parseInt(timestamp.replace("/Date(", "").replace(")/", ""), 10));
                        console.log('HTML TS: ' + convertDateTime(timestamp));
                        
                        console.log(timestamp);
                        */

                        console.log(data['Result']);
                    },
                    error: function (request, error) {
                        alert("Request: " + JSON.stringify(request));
                    }
                });

                //console.log(timestamp);

                //console.log(comments[comments.length - 1].getAttribute("timestamp"));
            }
        }
        
        //var comments = $('#' + commentDivs[commentDivs.length - 2].id).find('.commentTextClass');
        //console.log(comments[comments.length - 1])

        //console.log(commentDivs[commentDivs.length-1]);

        setTimeout(yourFunction, 30000);
    }

    yourFunction();
    

});