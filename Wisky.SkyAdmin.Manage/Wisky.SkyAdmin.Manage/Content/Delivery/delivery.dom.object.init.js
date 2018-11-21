function productItemDomObject(id, image, name, price, discount, type) {
    //console.log('abc');
    return $("<li/>", {
        'class': "serviceItem no-border",
        html: [$("<a/>", {
            'class': "img-back",
            'data-rel': "colorbox",
            'html': [$("<img/>", {
                'src': "/Content/images/" + image
            }), $("<div/>", {
                'class': "title",
                'html': name
            })]
        }), $("<div/>", {
            'class': "ace-spinner touch-spinner",
            'html': $("<div/>", {
                'class': "input-group",
                'html': [
                    $("<div/>", {
                        'class': "spinner-buttons input-group-btn",
                        'html': $("<button/>", {
                            'class': "btn btn-minus btn-xs radius-bottom-left",
                            'html': " <i class=\"fa fa-minus smaller-75\"></i>"
                        })
                    }),
                    "<input type=\"text\" class=\"input-sm input-quantity form-control\" value=\"1\" maxlength=\"3\">",
                    $("<div/>", {
                        'class': "spinner-buttons input-group-btn",
                        'html': $("<button/>", {
                            'class': "btn btn-order btn-xs radius-bottom-right",
                            'data-id':id,
                            'data-name':name,
                            'data-price':price,
                            'data-discount': discount,
                            'data-type': type,
                            'data-role': 'add-order-detail-btn',
                            html: "<i class=\"fa fa-check smaller-75\"></i>"
                        })
                    })
                ]
            })
        })]
    });
}

function orderDetailItemDomObject(id, name, price, discount, quantity) {
    return $("<tr/>", {
        "data-id": id,
        "data-role": "order-detail",
        html: [
            $("<td/>", {
                html: name
            }),
            $("<td/>", {
                html: price.toMoney(0, ',', '.')
            }),
            $("<td/>", {
                html: quantity
            }),
            $("<td/>", {
                html: discount
            }),
            $("<td/>", {
                html: (price * quantity * (100 - discount) / 100).toMoney(0, ',', '.')
            }),
            $("<td/>", {
                html: $("<button/>", {
                    "class": "btn btn-sm btn-danger",
                    "onclick": "deleteOrderDetail(" + id + ")"
                })
            })
        ]
    });
}