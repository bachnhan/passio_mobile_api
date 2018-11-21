function DeliveryOrder(callCenterId, submitUrl, checkPromotionUrl) {
    this.NextOrderDetailId = 1;
    this.SUBMIT_URL = submitUrl;
    this.CHECK_PROMOTION_URL = checkPromotionUrl;
    this.Order = {
        CallCenterID: callCenterId,
        OrderDetails: [],
        Customer: null,
        CustomerID: 0,
        StoreID: 0,
        BrandId: 0,
        RentStatus: null,
        RentType: 0,
        DeliveryStatus: 0,
        DeliveryAddress: "",
        TotalPayment: 0,
        Notes: '',
        //GiftIDs: new Array(),
        GiftNames: new Array(),
        GiftQuantity: new Array(),
        DiscountAmount: 0,
        DiscountPercent: 0,
        MembershipCardCode: "",
        WardCode: null,
        DistrictCode: null,
        ProvinceCode: null
    };
}

DeliveryOrder.prototype.addOrderDetail = function (data) {
    var totalAmount = data.price * data.quantity; 
    var extraMoney = 0;
    if (data.extra != null) {
        for (var i = 0; i < data.extra.length; i++) {
            extraMoney += data.extra[i].Money * data.extra[i].Quantity * data.quantity;
        }

    }
    var finalAmount = totalAmount * (100 - data.discount) / 100 + extraMoney;
    totalAmount += extraMoney;


    this.Order.OrderDetails.push({
        Id: this.NextOrderDetailId,
        Name: data.name,
        ProductID: data.id,
        Quantity: data.quantity,
        UnitPrice: data.price,
        TotalAmount: totalAmount,
        FinalAmount: finalAmount + 0.0,
        DiscountPercent: data.discount + 0.0,
        ProductType: data.type,
        Status: 2,
        //GiftIDs: new Array(),
        GiftNames: new Array(),
        GiftQuantity: new Array(),
        DiscountAmount: 0,
        Extra: data.extra,
        ExtraAmount: extraMoney
    });
    this.Order.TotalPayment = finalAmount;
    return this.NextOrderDetailId++;
};



DeliveryOrder.prototype.removeOrderDetail = function (orderDetailId) {
    this.Order.OrderDetails = this.Order.OrderDetails.filter(function (o, n) {
        return o.Id !== orderDetailId;
    });
};

DeliveryOrder.prototype.addNewCustomer = function (name, address, phone, email, notes) {
    this.Order.Customer = {
        Name: name,
        Address: address,
        Phone: phone,
        Email: email,
        Notes: notes,
        CustomerTypeId: 1,
    };
    //this.Order.Notes = note;
    this.Order.CustomerID = 0;
};

DeliveryOrder.prototype.addCustomerWithId = function (id, name, address, phone, email, notes) {
    this.Order.Customer = {
        Name: name,
        Address: address,
        Phone: phone,
        Email: email,
        Notes: notes,
        CustomerTypeId: 1,
    };
    //this.Order.Notes = note;
    this.Order.CustomerID = id;
};

DeliveryOrder.prototype.addWard = function (ward) {
    this.Order.WardCode = ward;
}

DeliveryOrder.prototype.addDistrict = function (district) {
    this.Order.DistrictCode = district;
}

DeliveryOrder.prototype.addProvince = function (province) {
    this.Order.ProvinceCode = province;
}

DeliveryOrder.prototype.addNote = function (note) {
    this.Order.Notes = note;
}

DeliveryOrder.prototype.addMembershipCardCode = function (code) {
    this.Order.MembershipCardCode = code;
}

DeliveryOrder.prototype.setSubmitUrl = function (url) {
    this.SUBMIT_URL = url;
}

DeliveryOrder.prototype.addExistedCustomer = function (customerId, note) {
    this.Order.Customer = null;
    this.Order.CustomerID = customerId;
    //this.Order.Notes = note;
}

DeliveryOrder.prototype.assignStore = function (storeId) {
    this.Order.StoreID = storeId;
}

DeliveryOrder.prototype.assignBrand = function (brandId) {
    this.Order.BrandId = brandId;
}

DeliveryOrder.prototype.isValidate = function () {

    var validPhone = /^([0-9]{5,})$/.test($('#cust-phone').val())
    var IsNewCustomer = ($("#btnIsNewUser").data("status") == "new");
    if ($('#cust-name').val() == "") {
        return {
            valid: false,
            msg: "Tên khách hàng không được để trống"
        }
    }
    else if ($('#cust-phone').val() == "") {
        return {
            valid: false,
            msg: "Số điện thoại khách hàng không được để trống"
        };
    }
    else if (!validPhone) {
        return {
            valid: false,
            msg: "Số điện thoại khách hàng không hợp lệ"
        };
    } else if (validPhone && IsNewCustomer) {
        for(var i = 0; i < window['customers-list'].length; i++) {
            if (window['customers-list'][i].phone == $('#cust-phone').val()) {
                return {
                    valid: false,
                    msg: "Số điện thoại đã tồn tại."
                };
            }
        }
    }
    else if ($('#txt-delivery-address').val() == "") {
        return {
            valid: false,
            msg: "Địa chỉ khách hàng không được để trống"
        };
    }
    else if (this.Order.DeliveryAddress == "") {
        return {
            valid: false,
            //msg: "Delivery address can not empty."
            msg: "Xin chọn cửa hàng để giao"
        };
    }
    if (this.Order.CustomerID == 0 && this.Order.Customer == null) {
        return {
            valid: false,
            //msg: "Customer info can not empty."
            msg: "Thông tin không được để trống."
        };
    }
    if (this.Order.Customer != null && this.Order.Customer.Name == null) {
        return {
            valid: false,
            //msg: "Customer name can not empty."
            msg: "Tên không được để trống."
        };
    }
    if (this.Order.Customer != null && this.Order.Customer.Phone == null) {
        return {
            valid: false,
            //msg: "Customer phone can not empty."
            msg: "Số điện thoại không được để trống."
        };
    } 
    if (this.Order.OrderDetails.length === 0) {
        return {
            valid: false,
            //msg: "Your order can not empty. Please order least one product."
            msg: "Không có sản phẩm trong đơn hàng. Hãy chọn ít nhất 1 sản phẩm."
        };
    }
    return {
        valid: true
    };
};

DeliveryOrder.prototype.submit = function (successCallback, failCallback) {
    $.ajax({
        url: this.SUBMIT_URL,
        type: "POST",
        data: JSON.stringify(this.Order),
        dataType: "json",
        contentType: "application/json",
        success: function (e) {
            if (e.success) {
                successCallback();
            } else {
                failCallback(e.msg);
            }
        },
        fail: function () {
            //failCallback("Error is  occur, please try again!");
            failCallback("Có lỗi xảy ra, vui lòng thử lại sau");
        }
    });
};
DeliveryOrder.prototype.submitTemp = function (successCallback, failCallback) {
    $.ajax({
        url: this.CHECK_PROMOTION_URL,
        type: "POST",
        data: JSON.stringify(this.Order),
        dataType: "json",
        contentType: "application/json",
        success: function (e) {
            if (e.success) {
                successCallback();
            } else {
                failCallback(e.msg);
            }
        },
        fail: function () {
            //failCallback("Error is  occur, please try again!");
            failCallback("Có lỗi xảy ra, vui lòng thử lại sau");

        }
    });
};

DeliveryOrder.prototype.getTotal = function () {
    var total = 0.0;
    for (var i = 0; i < this.Order.OrderDetails.length; i++) {
        total += this.Order.OrderDetails[i].FinalAmount;
    }
    total = total * (100 - this.Order.DiscountPercent) * 0.01;
    return total;
}

DeliveryOrder.prototype.getTotalNoDiscount = function () {
    var total = 0.0;
    for (var i = 0; i < this.Order.OrderDetails.length; i++) {
        total += this.Order.OrderDetails[i].TotalAmount;
    }
    return total;
}